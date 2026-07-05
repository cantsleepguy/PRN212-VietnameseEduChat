using System.Text.RegularExpressions;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class ResearchChunkingService : IResearchChunkingService
    {
        private readonly List<ResearchChunkingStrategyOptionDto> _strategies = new()
        {
            new ResearchChunkingStrategyOptionDto
            {
                Key = "fixed-small",
                Name = "Fixed-size small 800/100",
                ChunkSize = 800,
                ChunkOverlap = 100,
                Description = "Cắt theo số ký tự cố định, chunk nhỏ."
            },
            new ResearchChunkingStrategyOptionDto
            {
                Key = "fixed-baseline",
                Name = "Fixed-size baseline 1200/200",
                ChunkSize = 1200,
                ChunkOverlap = 200,
                Description = "Cấu hình baseline hiện tại."
            },
            new ResearchChunkingStrategyOptionDto
            {
                Key = "fixed-large",
                Name = "Fixed-size large 1600/300",
                ChunkSize = 1600,
                ChunkOverlap = 300,
                Description = "Cắt theo số ký tự cố định, chunk lớn."
            },
            new ResearchChunkingStrategyOptionDto
            {
                Key = "paragraph-based",
                Name = "Paragraph-based",
                ChunkSize = 1200,
                ChunkOverlap = 0,
                Description = "Ưu tiên giữ nguyên các đoạn văn."
            },
            new ResearchChunkingStrategyOptionDto
            {
                Key = "sentence-based",
                Name = "Sentence-based",
                ChunkSize = 1200,
                ChunkOverlap = 0,
                Description = "Ưu tiên giữ nguyên các câu."
            },
            new ResearchChunkingStrategyOptionDto
            {
                Key = "semantic-chunking",
                Name = "Semantic chunking",
                ChunkSize = 1200,
                ChunkOverlap = 0,
                Description = "MVP semantic chunking dựa trên thay đổi chủ đề giữa đoạn/câu."
            }
        };

        public List<ResearchChunkingStrategyOptionDto> GetStrategies()
        {
            return _strategies;
        }

        public ResearchChunkingStrategyOptionDto GetStrategy(string strategyKey)
        {
            var strategy = _strategies
                .FirstOrDefault(s => s.Key == strategyKey);

            if (strategy == null)
            {
                throw new InvalidOperationException(
                    $"Chunking strategy không hợp lệ: {strategyKey}");
            }

            return strategy;
        }

        public List<string> Chunk(string text, string strategyKey)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            var strategy = GetStrategy(strategyKey);

            return strategy.Key switch
            {
                "fixed-small" => FixedSizeChunk(text, 800, 100),
                "fixed-baseline" => FixedSizeChunk(text, 1200, 200),
                "fixed-large" => FixedSizeChunk(text, 1600, 300),
                "paragraph-based" => ParagraphBasedChunk(text, 1200),
                "sentence-based" => SentenceBasedChunk(text, 1200),
                "semantic-chunking" => SemanticChunkingMvp(text, 1200),
                _ => FixedSizeChunk(text, strategy.ChunkSize, strategy.ChunkOverlap)
            };
        }

        private static List<string> FixedSizeChunk(
            string text,
            int maxChunkLength,
            int overlap)
        {
            var chunks = new List<string>();

            var normalizedText = Regex
                .Replace(text, @"\s+", " ")
                .Trim();

            var step = maxChunkLength - overlap;

            if (step <= 0)
            {
                step = maxChunkLength;
            }

            for (var start = 0; start < normalizedText.Length; start += step)
            {
                var length = Math.Min(
                    maxChunkLength,
                    normalizedText.Length - start);

                var chunk = normalizedText
                    .Substring(start, length)
                    .Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(chunk);
                }

                if (start + length >= normalizedText.Length)
                {
                    break;
                }
            }

            return chunks;
        }

        private static List<string> ParagraphBasedChunk(
            string text,
            int maxChunkLength)
        {
            var paragraphs = SplitParagraphs(text);

            if (paragraphs.Count == 0)
            {
                return FixedSizeChunk(text, maxChunkLength, 0);
            }

            return GroupUnits(paragraphs, maxChunkLength);
        }

        private static List<string> SentenceBasedChunk(
            string text,
            int maxChunkLength)
        {
            var sentences = SplitSentences(text);

            if (sentences.Count == 0)
            {
                return FixedSizeChunk(text, maxChunkLength, 0);
            }

            return GroupUnits(sentences, maxChunkLength);
        }

        private static List<string> SemanticChunkingMvp(
            string text,
            int maxChunkLength)
        {
            var units = SplitParagraphs(text);

            if (units.Count < 3)
            {
                units = SplitSentences(text);
            }

            if (units.Count == 0)
            {
                return FixedSizeChunk(text, maxChunkLength, 0);
            }

            var chunks = new List<string>();
            var currentUnits = new List<string>();
            var currentLength = 0;

            foreach (var unit in units)
            {
                var shouldStartNewChunk = false;

                if (currentUnits.Count > 0)
                {
                    var currentText = string.Join(" ", currentUnits);
                    var similarity = LexicalSimilarity(currentText, unit);

                    if (similarity < 0.08 && currentLength >= maxChunkLength / 2)
                    {
                        shouldStartNewChunk = true;
                    }

                    if (currentLength + unit.Length > maxChunkLength)
                    {
                        shouldStartNewChunk = true;
                    }
                }

                if (shouldStartNewChunk)
                {
                    var chunk = string.Join(" ", currentUnits).Trim();

                    if (!string.IsNullOrWhiteSpace(chunk))
                    {
                        chunks.Add(chunk);
                    }

                    currentUnits.Clear();
                    currentLength = 0;
                }

                currentUnits.Add(unit);
                currentLength += unit.Length;
            }

            if (currentUnits.Count > 0)
            {
                var chunk = string.Join(" ", currentUnits).Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(chunk);
                }
            }

            return chunks;
        }

        private static List<string> GroupUnits(
            List<string> units,
            int maxChunkLength)
        {
            var chunks = new List<string>();
            var current = string.Empty;

            foreach (var unit in units)
            {
                if (string.IsNullOrWhiteSpace(current))
                {
                    current = unit;
                    continue;
                }

                if (current.Length + unit.Length + 1 <= maxChunkLength)
                {
                    current += " " + unit;
                }
                else
                {
                    chunks.Add(current.Trim());
                    current = unit;
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                chunks.Add(current.Trim());
            }

            return chunks;
        }

        private static List<string> SplitParagraphs(string text)
        {
            var normalized = text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");

            return Regex
                .Split(normalized, @"\n\s*\n+")
                .Select(p => Regex.Replace(p, @"\s+", " ").Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
        }

        private static List<string> SplitSentences(string text)
        {
            var normalized = Regex
                .Replace(text, @"\s+", " ")
                .Trim();

            return Regex
                .Split(normalized, @"(?<=[\.!\?。！？])\s+")
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        private static double LexicalSimilarity(
            string textA,
            string textB)
        {
            var termsA = ExtractTerms(textA).ToHashSet();
            var termsB = ExtractTerms(textB).ToHashSet();

            if (termsA.Count == 0 || termsB.Count == 0)
            {
                return 0;
            }

            var intersection = termsA.Intersect(termsB).Count();
            var union = termsA.Union(termsB).Count();

            return union == 0
                ? 0
                : (double)intersection / union;
        }

        private static List<string> ExtractTerms(string text)
        {
            var normalized = new string(
                text.ToLowerInvariant()
                    .Select(c =>
                        char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)
                            ? c
                            : ' ')
                    .ToArray());

            return normalized
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(term => term.Length >= 3)
                .Distinct()
                .ToList();
        }
    }
}