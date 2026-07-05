using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class ResearchBenchmarkService : IResearchBenchmarkService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IResearchIndexService _researchIndexService;
        private readonly IResearchChunkingService _researchChunkingService;

        public ResearchBenchmarkService(
            ApplicationDbContext context,
            IEmbeddingService embeddingService,
            IChatCompletionService chatCompletionService,
            IResearchIndexService researchIndexService,
            IResearchChunkingService researchChunkingService)
        {
            _context = context;
            _embeddingService = embeddingService;
            _chatCompletionService = chatCompletionService;
            _researchIndexService = researchIndexService;
            _researchChunkingService = researchChunkingService;
        }

        public async Task<List<ResearchExperimentDto>> GetExperimentsAsync()
        {
            var totalQuestions = await _context.ResearchQuestions.CountAsync();

            return await _context.ResearchExperiments
                .Include(e => e.Results)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new ResearchExperimentDto
                {
                    ResearchExperimentId = e.ResearchExperimentId,
                    ExperimentName = e.ExperimentName,
                    ExperimentType = e.ExperimentType,
                    AnswerModelName = e.AnswerModelName,
                    EmbeddingProvider = e.EmbeddingProvider,
                    EmbeddingModelName = e.EmbeddingModelName,
                    EmbeddingDimensions = e.EmbeddingDimensions,
                    ChunkingStrategyKey = e.ChunkingStrategyKey,
                    ChunkingStrategyName = e.ChunkingStrategyName,
                    ChunkSize = e.ChunkSize,
                    ChunkOverlap = e.ChunkOverlap,
                    TopK = e.TopK,
                    Status = e.Status,
                    TotalQuestions = totalQuestions,
                    CompletedResults = e.Results.Count,
                    AverageAnswerSimilarity = e.Results.Any()
                        ? e.Results.Average(r => r.AnswerSimilarityScore)
                        : 0,
                    AverageContextRelevance = e.Results.Any()
                        ? e.Results.Average(r => r.ContextRelevanceScore)
                        : 0,
                    AverageGroundedness = e.Results.Any()
                        ? e.Results.Average(r => r.GroundednessScore)
                        : 0,
                    AverageKeywordHit = e.Results.Any()
                        ? e.Results.Average(r => r.KeywordHitScore)
                        : 0,
                    AverageOverallScore = e.Results.Any()
                        ? e.Results.Average(r => r.OverallScore)
                        : 0,
                    CreatedAt = e.CreatedAt,
                    StartedAt = e.StartedAt,
                    FinishedAt = e.FinishedAt
                })
                .ToListAsync();
        }

        public async Task<ResearchExperimentDetailDto?> GetExperimentDetailAsync(
            int experimentId)
        {
            var experiment = await _context.ResearchExperiments
                .Include(e => e.Results)
                    .ThenInclude(r => r.ResearchQuestion)
                .FirstOrDefaultAsync(e => e.ResearchExperimentId == experimentId);

            if (experiment == null)
            {
                return null;
            }

            var totalQuestions = await _context.ResearchQuestions.CountAsync();

            var experimentDto = new ResearchExperimentDto
            {
                ResearchExperimentId = experiment.ResearchExperimentId,
                ExperimentName = experiment.ExperimentName,
                ExperimentType = experiment.ExperimentType,
                AnswerModelName = experiment.AnswerModelName,
                EmbeddingProvider = experiment.EmbeddingProvider,
                EmbeddingModelName = experiment.EmbeddingModelName,
                EmbeddingDimensions = experiment.EmbeddingDimensions,
                ChunkingStrategyKey = experiment.ChunkingStrategyKey,
                ChunkingStrategyName = experiment.ChunkingStrategyName,
                ChunkSize = experiment.ChunkSize,
                ChunkOverlap = experiment.ChunkOverlap,
                TopK = experiment.TopK,
                Status = experiment.Status,
                TotalQuestions = totalQuestions,
                CompletedResults = experiment.Results.Count,
                AverageAnswerSimilarity = experiment.Results.Any()
                    ? experiment.Results.Average(r => r.AnswerSimilarityScore)
                    : 0,
                AverageContextRelevance = experiment.Results.Any()
                    ? experiment.Results.Average(r => r.ContextRelevanceScore)
                    : 0,
                AverageGroundedness = experiment.Results.Any()
                    ? experiment.Results.Average(r => r.GroundednessScore)
                    : 0,
                AverageKeywordHit = experiment.Results.Any()
                    ? experiment.Results.Average(r => r.KeywordHitScore)
                    : 0,
                AverageOverallScore = experiment.Results.Any()
                    ? experiment.Results.Average(r => r.OverallScore)
                    : 0,
                CreatedAt = experiment.CreatedAt,
                StartedAt = experiment.StartedAt,
                FinishedAt = experiment.FinishedAt
            };

            var resultDtos = experiment.Results
                .OrderBy(r => r.ResearchQuestionId)
                .Select(r => new ResearchResultDto
                {
                    ResearchResultId = r.ResearchResultId,
                    ResearchQuestionId = r.ResearchQuestionId,
                    Question = r.ResearchQuestion?.Question ?? string.Empty,
                    GroundTruthAnswer = r.ResearchQuestion?.GroundTruthAnswer ?? string.Empty,
                    GeneratedAnswer = r.GeneratedAnswer,
                    RetrievedSourcesJson = r.RetrievedSourcesJson,
                    AnswerSimilarityScore = r.AnswerSimilarityScore,
                    ContextRelevanceScore = r.ContextRelevanceScore,
                    GroundednessScore = r.GroundednessScore,
                    KeywordHitScore = r.KeywordHitScore,
                    OverallScore = r.OverallScore,
                    LatencyMs = r.LatencyMs,
                    ErrorMessage = r.ErrorMessage,
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            return new ResearchExperimentDetailDto
            {
                Experiment = experimentDto,
                Results = resultDtos
            };
        }

        public async Task<int> CreateExperimentAsync(
            ResearchExperimentCreateDto input)
        {
            if (string.IsNullOrWhiteSpace(input.ExperimentName))
            {
                throw new InvalidOperationException(
                    "Tên experiment không được để trống.");
            }

            var strategy = _researchChunkingService.GetStrategy(
                input.ChunkingStrategyKey);

            var embeddingModelName = string.IsNullOrWhiteSpace(input.EmbeddingModelName)
                ? _embeddingService.GetModelName()
                : input.EmbeddingModelName.Trim();

            var embeddingDimensions = input.EmbeddingDimensions > 0
                ? input.EmbeddingDimensions
                : _embeddingService.GetDimensions(embeddingModelName);

            var experiment = new ResearchExperiment
            {
                ExperimentName = input.ExperimentName.Trim(),
                ExperimentType = string.IsNullOrWhiteSpace(input.ExperimentType)
                    ? "RAG"
                    : input.ExperimentType.Trim(),

                AnswerModelName = string.IsNullOrWhiteSpace(input.AnswerModelName)
                    ? "gpt-4o-mini"
                    : input.AnswerModelName.Trim(),

                EmbeddingProvider = string.IsNullOrWhiteSpace(input.EmbeddingProvider)
                    ? "OpenAI"
                    : input.EmbeddingProvider.Trim(),

                EmbeddingModelName = embeddingModelName,

                EmbeddingDimensions = embeddingDimensions,

                ChunkingStrategyKey = strategy.Key,
                ChunkingStrategyName = strategy.Name,
                ChunkSize = strategy.ChunkSize,
                ChunkOverlap = strategy.ChunkOverlap,

                TopK = input.TopK <= 0 ? 5 : input.TopK,

                Status = "Pending",
                CreatedAt = DateTime.Now,
                Notes = input.Notes?.Trim()
            };

            _context.ResearchExperiments.Add(experiment);
            await _context.SaveChangesAsync();

            return experiment.ResearchExperimentId;
        }

        public async Task RunExperimentAsync(int experimentId)
        {
            var experiment = await _context.ResearchExperiments
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.ResearchExperimentId == experimentId);

            if (experiment == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy experiment.");
            }

            if (experiment.Status == "Running")
            {
                throw new InvalidOperationException(
                    "Experiment này đang chạy.");
            }

            var questions = await _context.ResearchQuestions
                .Include(q => q.Subject)
                .Include(q => q.SourceDocument)
                .OrderBy(q => q.ResearchQuestionId)
                .ToListAsync();

            if (questions.Count == 0)
            {
                throw new InvalidOperationException(
                    "Chưa có câu hỏi nào trong test set.");
            }

            _context.ResearchResults.RemoveRange(experiment.Results);

            experiment.Status = "Running";
            experiment.StartedAt = DateTime.Now;
            experiment.FinishedAt = null;

            await _context.SaveChangesAsync();

            foreach (var question in questions)
            {
                var stopwatch = Stopwatch.StartNew();

                var result = new ResearchResult
                {
                    ResearchExperimentId = experiment.ResearchExperimentId,
                    ResearchQuestionId = question.ResearchQuestionId,
                    CreatedAt = DateTime.Now
                };

                try
                {
                    await _researchIndexService.EnsureIndexedAsync(
                        question.SubjectId,
                        question.SourceDocumentId,
                        experiment.ChunkingStrategyKey,
                        experiment.EmbeddingProvider,
                        experiment.EmbeddingModelName);

                    var questionEmbedding =
                        await _embeddingService.CreateEmbeddingAsync(
                            question.Question,
                            experiment.EmbeddingModelName,
                            experiment.EmbeddingDimensions > 0
                                ? experiment.EmbeddingDimensions
                                : null);

                    var relevantChunks = await _researchIndexService.SearchRelevantChunksAsync(
                        questionEmbedding,
                        question.SubjectId,
                        question.SourceDocumentId,
                        experiment.ChunkingStrategyKey,
                        experiment.EmbeddingModelName,
                        experiment.TopK);

                    if (relevantChunks.Count == 0)
                    {
                        result.GeneratedAnswer =
                            "Không tìm thấy context phù hợp trong tài liệu.";

                        result.RetrievedContext = string.Empty;
                        result.RetrievedSourcesJson = "[]";
                        result.ErrorMessage = "No relevant chunks found.";
                    }
                    else
                    {
                        var prompt = BuildBenchmarkPrompt(
                            question.Question,
                            relevantChunks);

                        var answer = await _chatCompletionService
                            .GenerateAnswerAsync(prompt);

                        result.GeneratedAnswer = answer;
                        result.RetrievedContext = BuildRetrievedContext(relevantChunks);

                        result.RetrievedSourcesJson = JsonSerializer.Serialize(
                            relevantChunks.Select(c => new
                            {
                                c.ResearchDocumentChunkId,
                                c.DocumentId,
                                c.DocumentName,
                                c.ChunkIndex,
                                c.SimilarityScore
                            }));
                    }

                    result.ContextRelevanceScore =
                        CalculateContextRelevanceScore(relevantChunks);

                    result.KeywordHitScore =
                        CalculateKeywordHitScore(
                            result.GeneratedAnswer,
                            question.ExpectedKeywords);

                    result.GroundednessScore =
                        CalculateGroundednessScore(
                            result.GeneratedAnswer,
                            result.RetrievedContext);

                    result.AnswerSimilarityScore =
                        await CalculateAnswerSimilarityScoreAsync(
                            result.GeneratedAnswer,
                            question.GroundTruthAnswer,
                            experiment.EmbeddingModelName,
                            experiment.EmbeddingDimensions);

                    result.OverallScore = CalculateOverallScore(
                        result.AnswerSimilarityScore,
                        result.ContextRelevanceScore,
                        result.GroundednessScore,
                        result.KeywordHitScore);
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = ex.Message;
                    result.GeneratedAnswer = string.Empty;
                    result.RetrievedContext = string.Empty;
                    result.RetrievedSourcesJson = "[]";
                }
                finally
                {
                    stopwatch.Stop();
                    result.LatencyMs = stopwatch.ElapsedMilliseconds;
                }

                _context.ResearchResults.Add(result);

                await _context.SaveChangesAsync();
            }

            experiment.Status = "Completed";
            experiment.FinishedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteExperimentAsync(int experimentId)
        {
            var experiment = await _context.ResearchExperiments
                .FirstOrDefaultAsync(e => e.ResearchExperimentId == experimentId);

            if (experiment == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy experiment.");
            }

            _context.ResearchExperiments.Remove(experiment);

            await _context.SaveChangesAsync();
        }

        private static string BuildBenchmarkPrompt(
            string question,
            List<ResearchScoredChunkDto> relevantChunks)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Bạn là chatbot hỗ trợ học tập cho sinh viên.");
            builder.AppendLine("Chỉ trả lời dựa trên CONTEXT tài liệu được cung cấp.");
            builder.AppendLine("Nếu CONTEXT không đủ thông tin, hãy nói: \"Tôi không tìm thấy thông tin này trong tài liệu.\"");
            builder.AppendLine("Trả lời ngắn gọn, rõ ràng, bằng tiếng Việt.");
            builder.AppendLine();

            builder.AppendLine("===== CONTEXT TÀI LIỆU =====");

            for (var i = 0; i < relevantChunks.Count; i++)
            {
                var chunk = relevantChunks[i];

                builder.AppendLine($"[Nguồn {i + 1}]");
                builder.AppendLine($"File: {chunk.DocumentName}");
                builder.AppendLine($"ChunkIndex: {chunk.ChunkIndex}");
                builder.AppendLine(chunk.Content);
                builder.AppendLine();
            }

            builder.AppendLine("===== QUESTION =====");
            builder.AppendLine(question);
            builder.AppendLine();

            builder.AppendLine("===== ANSWER =====");

            return builder.ToString();
        }

        private static string BuildRetrievedContext(
            List<ResearchScoredChunkDto> chunks)
        {
            var builder = new StringBuilder();

            foreach (var chunk in chunks)
            {
                builder.AppendLine($"File: {chunk.DocumentName}");
                builder.AppendLine($"ChunkIndex: {chunk.ChunkIndex}");
                builder.AppendLine(chunk.Content);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static double CalculateContextRelevanceScore(
            List<ResearchScoredChunkDto> chunks)
        {
            if (chunks.Count == 0)
            {
                return 0;
            }

            return Clamp01(chunks.Average(c => c.SimilarityScore));
        }

        private static double CalculateKeywordHitScore(
            string generatedAnswer,
            string? expectedKeywords)
        {
            if (string.IsNullOrWhiteSpace(expectedKeywords))
            {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(generatedAnswer))
            {
                return 0;
            }

            var keywords = expectedKeywords
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToLowerInvariant())
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct()
                .ToList();

            if (keywords.Count == 0)
            {
                return 0;
            }

            var answer = generatedAnswer.ToLowerInvariant();

            var hitCount = keywords.Count(keyword =>
                answer.Contains(keyword));

            return (double)hitCount / keywords.Count;
        }

        private static double CalculateGroundednessScore(
            string generatedAnswer,
            string retrievedContext)
        {
            if (string.IsNullOrWhiteSpace(generatedAnswer) ||
                string.IsNullOrWhiteSpace(retrievedContext))
            {
                return 0;
            }

            var answerTerms = ExtractTerms(generatedAnswer);
            var contextTerms = ExtractTerms(retrievedContext).ToHashSet();

            if (answerTerms.Count == 0)
            {
                return 0;
            }

            var supportedTerms = answerTerms.Count(term =>
                contextTerms.Contains(term));

            return (double)supportedTerms / answerTerms.Count;
        }

        private async Task<double> CalculateAnswerSimilarityScoreAsync(
            string generatedAnswer,
            string groundTruthAnswer,
            string embeddingModelName,
            int embeddingDimensions)
        {
            if (string.IsNullOrWhiteSpace(generatedAnswer) ||
                string.IsNullOrWhiteSpace(groundTruthAnswer))
            {
                return 0;
            }

            var dimensions = embeddingDimensions > 0
                ? embeddingDimensions
                : _embeddingService.GetDimensions(embeddingModelName);

            var generatedEmbedding =
                await _embeddingService.CreateEmbeddingAsync(
                    generatedAnswer,
                    embeddingModelName,
                    dimensions);

            var groundTruthEmbedding =
                await _embeddingService.CreateEmbeddingAsync(
                    groundTruthAnswer,
                    embeddingModelName,
                    dimensions);

            return Clamp01(CosineSimilarity(
                generatedEmbedding,
                groundTruthEmbedding));
        }

        private static double CalculateOverallScore(
            double answerSimilarity,
            double contextRelevance,
            double groundedness,
            double keywordHit)
        {
            return
                answerSimilarity * 0.4 +
                contextRelevance * 0.2 +
                groundedness * 0.2 +
                keywordHit * 0.2;
        }

        private static List<string> ExtractTerms(string text)
        {
            var normalized = new string(
                text.ToLowerInvariant()
                    .Select(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)
                        ? c
                        : ' ')
                    .ToArray());

            return normalized
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(term => term.Length >= 3)
                .Distinct()
                .ToList();
        }

        private static double Clamp01(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return 0;
            }

            if (value < 0)
            {
                return 0;
            }

            if (value > 1)
            {
                return 1;
            }

            return value;
        }

        private static double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length == 0 ||
                vectorB.Length == 0 ||
                vectorA.Length != vectorB.Length)
            {
                return 0;
            }

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (var i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0;
            }

            return dotProduct /
                   (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }
    }
}