using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Documents;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class ChunkService : IChunkService
    {
        private const int DefaultChunkLength = 1200;
        private const int DefaultOverlap = 200;

        /// <summary>
        /// Ước lượng 1 token ~ 4 ký tự đối với văn bản tiếng Việt/Anh.
        /// </summary>
        private const int ApproxCharsPerToken = 4;

        private static readonly Regex PageMarkerRegex = new(
            @"^(?:=====\s*PDF\s*-\s*Trang\s+(\d+)\s*=====|-{3,}\s*OCR\s+PDF\s*-\s*Trang\s+(\d+)\s*-{3,})\s*$",
            RegexOptions.Multiline | RegexOptions.Compiled);

        public List<string> Chunk(string text)
        {
            return ChunkByCharacter(
                    text,
                    DefaultChunkLength,
                    DefaultOverlap)
                .Select(chunk => chunk.Content)
                .ToList();
        }

        public List<ChunkResultDto> Chunk(
            string text,
            ChunkingConfiguration configuration)
        {
            var results = new List<ChunkResultDto>();

            if (string.IsNullOrWhiteSpace(text))
                return results;

            var segments = SplitIntoPageSegments(text);

            foreach (var segment in segments)
            {
                var segmentChunks = configuration.StrategyKey switch
                {
                    ChunkingConfigurationService.StrategyParagraph =>
                        ChunkByParagraph(
                            segment.Content,
                            configuration.ChunkSize),

                    ChunkingConfigurationService.StrategyFixedSize =>
                        ChunkByFixedSize(
                            segment.Content,
                            configuration.ChunkSize,
                            configuration.ChunkOverlap,
                            configuration.FixedSizeUnit),

                    _ => ChunkByCharacter(
                            segment.Content,
                            configuration.ChunkSize,
                            configuration.ChunkOverlap)
                };

                foreach (var chunk in segmentChunks)
                {
                    chunk.PageNumber = segment.PageNumber;
                    results.Add(chunk);
                }
            }

            return results;
        }

        private static List<PageSegment> SplitIntoPageSegments(string text)
        {
            var segments = new List<PageSegment>();

            var matches = PageMarkerRegex.Matches(text);

            if (matches.Count == 0)
            {
                segments.Add(new PageSegment
                {
                    PageNumber = null,
                    Content = text
                });

                return segments;
            }

            var preamble = text.Substring(0, matches[0].Index);

            if (!string.IsNullOrWhiteSpace(preamble))
            {
                segments.Add(new PageSegment
                {
                    PageNumber = null,
                    Content = preamble
                });
            }

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                var contentStart = match.Index + match.Length;

                var contentEnd = i + 1 < matches.Count
                    ? matches[i + 1].Index
                    : text.Length;

                var content = text.Substring(
                    contentStart,
                    contentEnd - contentStart);

                if (string.IsNullOrWhiteSpace(content))
                    continue;

                var pageValue = match.Groups[1].Success
                    ? match.Groups[1].Value
                    : match.Groups[2].Value;

                int? pageNumber = int.TryParse(pageValue, out var parsed)
                    ? parsed
                    : null;

                segments.Add(new PageSegment
                {
                    PageNumber = pageNumber,
                    Content = content
                });
            }

            return segments;
        }

        private static List<ChunkResultDto> ChunkByCharacter(
            string text,
            int maxChunkLength,
            int overlap)
        {
            var chunks = new List<ChunkResultDto>();

            if (string.IsNullOrWhiteSpace(text))
                return chunks;

            if (maxChunkLength <= 0)
                maxChunkLength = DefaultChunkLength;

            if (overlap < 0 || overlap >= maxChunkLength)
                overlap = 0;

            var normalizedText = Regex
                .Replace(text, @"\s+", " ")
                .Trim();

            var step = maxChunkLength - overlap;

            for (int start = 0; start < normalizedText.Length; start += step)
            {
                var length = Math.Min(
                    maxChunkLength,
                    normalizedText.Length - start);

                var chunk = normalizedText
                    .Substring(start, length)
                    .Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(new ChunkResultDto
                    {
                        Content = chunk
                    });
                }

                if (start + length >= normalizedText.Length)
                    break;
            }

            return chunks;
        }

        private static List<ChunkResultDto> ChunkByParagraph(
            string text,
            int maxChunkLength)
        {
            var chunks = new List<ChunkResultDto>();

            if (string.IsNullOrWhiteSpace(text))
                return chunks;

            if (maxChunkLength <= 0)
                maxChunkLength = DefaultChunkLength;

            var paragraphs = Regex
                .Split(text, @"(?:\r?\n\s*){2,}")
                .Select(p => Regex.Replace(p, @"\s+", " ").Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            var builder = new StringBuilder();

            foreach (var paragraph in paragraphs)
            {
                if (paragraph.Length > maxChunkLength)
                {
                    FlushBuilder(builder, chunks);

                    foreach (var piece in ChunkByCharacter(
                        paragraph,
                        maxChunkLength,
                        0))
                    {
                        chunks.Add(piece);
                    }

                    continue;
                }

                var wouldExceed =
                    builder.Length > 0 &&
                    builder.Length + paragraph.Length + 1 > maxChunkLength;

                if (wouldExceed)
                {
                    FlushBuilder(builder, chunks);
                }

                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(paragraph);
            }

            FlushBuilder(builder, chunks);

            return chunks;
        }

        private static List<ChunkResultDto> ChunkByFixedSize(
            string text,
            int chunkSize,
            int chunkOverlap,
            string fixedSizeUnit)
        {
            if (string.Equals(
                fixedSizeUnit,
                "Tokens",
                StringComparison.OrdinalIgnoreCase))
            {
                return ChunkByTokens(text, chunkSize, chunkOverlap);
            }

            return ChunkByCharacter(text, chunkSize, chunkOverlap);
        }

        private static List<ChunkResultDto> ChunkByTokens(
            string text,
            int maxTokens,
            int overlapTokens)
        {
            var chunks = new List<ChunkResultDto>();

            if (string.IsNullOrWhiteSpace(text))
                return chunks;

            if (maxTokens <= 0)
                maxTokens = DefaultChunkLength / ApproxCharsPerToken;

            if (overlapTokens < 0 || overlapTokens >= maxTokens)
                overlapTokens = 0;

            var words = Regex
                .Split(text.Trim(), @"\s+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToArray();

            if (words.Length == 0)
                return chunks;

            // Ước lượng: trung bình 1 từ tiếng Việt ~ 1.3 token.
            var wordsPerChunk = Math.Max(
                1,
                (int)(maxTokens / 1.3));

            var overlapWords = Math.Max(
                0,
                (int)(overlapTokens / 1.3));

            if (overlapWords >= wordsPerChunk)
                overlapWords = 0;

            var step = wordsPerChunk - overlapWords;

            for (int start = 0; start < words.Length; start += step)
            {
                var count = Math.Min(
                    wordsPerChunk,
                    words.Length - start);

                var chunk = string
                    .Join(' ', words.Skip(start).Take(count))
                    .Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(new ChunkResultDto
                    {
                        Content = chunk
                    });
                }

                if (start + count >= words.Length)
                    break;
            }

            return chunks;
        }

        private static void FlushBuilder(
            StringBuilder builder,
            List<ChunkResultDto> chunks)
        {
            if (builder.Length == 0)
                return;

            var content = builder.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(content))
            {
                chunks.Add(new ChunkResultDto
                {
                    Content = content
                });
            }

            builder.Clear();
        }

        private class PageSegment
        {
            public int? PageNumber { get; set; }

            public string Content { get; set; } = string.Empty;
        }
    }
}
