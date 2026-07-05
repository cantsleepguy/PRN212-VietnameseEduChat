using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IResearchIndexService
    {
        Task EnsureIndexedAsync(
            int? subjectId,
            int? sourceDocumentId,
            string chunkingStrategyKey,
            string embeddingProvider,
            string embeddingModelName);

        Task<List<ResearchScoredChunkDto>> SearchRelevantChunksAsync(
            float[] questionEmbedding,
            int? subjectId,
            int? sourceDocumentId,
            string chunkingStrategyKey,
            string embeddingModelName,
            int topK);
    }
}