using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System.Text.Json;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class ResearchIndexService : IResearchIndexService
    {
        private const double MinimumSimilarityScore = 0.15;

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ITextExtractorService _textExtractorService;
        private readonly IResearchChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;

        public ResearchIndexService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ITextExtractorService textExtractorService,
            IResearchChunkingService chunkingService,
            IEmbeddingService embeddingService)
        {
            _context = context;
            _environment = environment;
            _textExtractorService = textExtractorService;
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
        }

        public async Task EnsureIndexedAsync(
            int? subjectId,
            int? sourceDocumentId,
            string chunkingStrategyKey,
            string embeddingProvider,
            string embeddingModelName)
        {
            var currentEmbeddingModel = _embeddingService.GetModelName();

            if (!string.Equals(
                    embeddingModelName,
                    currentEmbeddingModel,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Phase 1 hiện chỉ hỗ trợ embedding model đang cấu hình: " +
                    currentEmbeddingModel +
                    ". Phase 2 sẽ mở rộng chọn text-embedding-3-small / text-embedding-3-large thật.");
            }

            var strategy = _chunkingService.GetStrategy(chunkingStrategyKey);

            var query = _context.Documents
                .Where(d => d.Status == "Approved");

            if (sourceDocumentId.HasValue)
            {
                query = query.Where(d =>
                    d.DocumentId == sourceDocumentId.Value);
            }
            else if (subjectId.HasValue)
            {
                query = query.Where(d =>
                    d.SubjectId == subjectId.Value);
            }

            var documents = await query.ToListAsync();

            foreach (var document in documents)
            {
                var alreadyIndexed = await _context.ResearchDocumentChunks
                    .AnyAsync(c =>
                        c.DocumentId == document.DocumentId &&
                        c.ChunkingStrategyKey == strategy.Key &&
                        c.EmbeddingModelName == embeddingModelName);

                if (alreadyIndexed)
                {
                    continue;
                }

                await IndexDocumentAsync(
                    document,
                    strategy,
                    embeddingProvider,
                    embeddingModelName);
            }
        }

        public async Task<List<ResearchScoredChunkDto>> SearchRelevantChunksAsync(
            float[] questionEmbedding,
            int? subjectId,
            int? sourceDocumentId,
            string chunkingStrategyKey,
            string embeddingModelName,
            int topK)
        {
            var query = _context.ResearchDocumentChunks
                .Include(c => c.Document)
                .Where(c =>
                    c.Document != null &&
                    c.Document.Status == "Approved" &&
                    c.ChunkingStrategyKey == chunkingStrategyKey &&
                    c.EmbeddingModelName == embeddingModelName &&
                    !string.IsNullOrWhiteSpace(c.EmbeddingJson));

            if (sourceDocumentId.HasValue)
            {
                query = query.Where(c =>
                    c.DocumentId == sourceDocumentId.Value);
            }
            else if (subjectId.HasValue)
            {
                query = query.Where(c =>
                    c.Document != null &&
                    c.Document.SubjectId == subjectId.Value);
            }

            var chunks = await query
                .Select(c => new
                {
                    c.ResearchDocumentChunkId,
                    c.DocumentId,
                    c.ChunkIndex,
                    c.Content,
                    c.EmbeddingJson,
                    DocumentName = c.Document != null
                        ? c.Document.OriginalFileName
                        : null
                })
                .ToListAsync();

            var scoredChunks = new List<ResearchScoredChunkDto>();

            foreach (var chunk in chunks)
            {
                var chunkEmbedding = DeserializeEmbedding(
                    chunk.EmbeddingJson);

                if (chunkEmbedding.Length == 0)
                {
                    continue;
                }

                var similarity = CosineSimilarity(
                    questionEmbedding,
                    chunkEmbedding);

                if (similarity < MinimumSimilarityScore)
                {
                    continue;
                }

                scoredChunks.Add(new ResearchScoredChunkDto
                {
                    ResearchDocumentChunkId = chunk.ResearchDocumentChunkId,
                    DocumentId = chunk.DocumentId,
                    ChunkIndex = chunk.ChunkIndex,
                    Content = chunk.Content,
                    DocumentName = chunk.DocumentName,
                    SimilarityScore = similarity
                });
            }

            return scoredChunks
                .OrderByDescending(c => c.SimilarityScore)
                .Take(topK)
                .ToList();
        }

        private async Task IndexDocumentAsync(
            Document document,
            ResearchChunkingStrategyOptionDto strategy,
            string embeddingProvider,
            string embeddingModelName)
        {
            var physicalPath = Path.Combine(
                _environment.WebRootPath,
                document.FilePath.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(physicalPath))
            {
                throw new FileNotFoundException(
                    $"Không tìm thấy file gốc: {physicalPath}");
            }

            var extension = Path
                .GetExtension(document.OriginalFileName)
                .ToLowerInvariant();

            var text = await _textExtractorService.ExtractAsync(
                physicalPath,
                extension);

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException(
                    $"Không thể đọc nội dung tài liệu: {document.OriginalFileName}");
            }

            var chunks = _chunkingService.Chunk(
                text,
                strategy.Key);

            if (chunks.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Không tạo được benchmark chunks cho tài liệu: {document.OriginalFileName}");
            }

            var entities = new List<ResearchDocumentChunk>();

            for (var i = 0; i < chunks.Count; i++)
            {
                var embedding = await _embeddingService
                    .CreateEmbeddingAsync(chunks[i]);

                entities.Add(new ResearchDocumentChunk
                {
                    DocumentId = document.DocumentId,
                    ChunkingStrategyKey = strategy.Key,
                    ChunkingStrategyName = strategy.Name,
                    ChunkSize = strategy.ChunkSize,
                    ChunkOverlap = strategy.ChunkOverlap,
                    ChunkIndex = i + 1,
                    Content = chunks[i],
                    EmbeddingProvider = embeddingProvider,
                    EmbeddingModelName = embeddingModelName,
                    EmbeddingDimensions = embedding.Length,
                    EmbeddingJson = JsonSerializer.Serialize(embedding),
                    CreatedAt = DateTime.Now
                });
            }

            _context.ResearchDocumentChunks.AddRange(entities);

            await _context.SaveChangesAsync();
        }

        private static float[] DeserializeEmbedding(string embeddingJson)
        {
            if (string.IsNullOrWhiteSpace(embeddingJson))
            {
                return Array.Empty<float>();
            }

            try
            {
                var floatArray = JsonSerializer.Deserialize<float[]>(
                    embeddingJson);

                return floatArray ?? Array.Empty<float>();
            }
            catch
            {
                return Array.Empty<float>();
            }
        }

        private static double CosineSimilarity(
            float[] vectorA,
            float[] vectorB)
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