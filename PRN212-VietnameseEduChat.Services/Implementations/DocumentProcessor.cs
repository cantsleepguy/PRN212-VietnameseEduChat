using System.Text.Json;
using Microsoft.Extensions.Logging;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Services.Implementations;

public sealed class DocumentProcessor : IDocumentProcessor
{
    private const string SafeFailureMessage =
        "Không thể xử lý tài liệu. Vui lòng kiểm tra file và thử lại.";

    private readonly IDocumentRepository _repository;
    private readonly IDocumentStorage _storage;
    private readonly ITextExtractorService _textExtractor;
    private readonly IChunkService _chunkService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IChunkingConfigurationService _chunkingConfiguration;
    private readonly ILogger<DocumentProcessor> _logger;

    public DocumentProcessor(
        IDocumentRepository repository,
        IDocumentStorage storage,
        ITextExtractorService textExtractor,
        IChunkService chunkService,
        IEmbeddingService embeddingService,
        IChunkingConfigurationService chunkingConfiguration,
        ILogger<DocumentProcessor> logger)
    {
        _repository = repository;
        _storage = storage;
        _textExtractor = textExtractor;
        _chunkService = chunkService;
        _embeddingService = embeddingService;
        _chunkingConfiguration = chunkingConfiguration;
        _logger = logger;
    }

    public async Task ProcessAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _repository.GetByIdAsync(documentId);
        if (document == null || document.Status != "Queued")
        {
            return;
        }

        document.Status = "Processing";
        document.ErrorMessage = null;
        await _repository.UpdateAsync(document);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = _storage.GetPhysicalPath(document.StoredFileName);
            if (!File.Exists(path))
            {
                throw new InvalidOperationException("Source document is missing.");
            }

            var extension = Path.GetExtension(document.StoredFileName).ToLowerInvariant();
            var text = await _textExtractor.ExtractAsync(path, extension);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("Extracted document text is empty.");
            }

            var configuration = await _chunkingConfiguration.GetActiveAsync();
            var chunks = _chunkService.Chunk(text, configuration);
            if (chunks.Count == 0)
            {
                throw new InvalidOperationException("Document produced no chunks.");
            }

            var entities = new List<DocumentChunk>(chunks.Count);
            for (var index = 0; index < chunks.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var embedding = await _embeddingService.CreateEmbeddingAsync(chunks[index].Content);
                entities.Add(new DocumentChunk
                {
                    DocumentId = document.DocumentId,
                    ChunkIndex = index + 1,
                    PageNumber = chunks[index].PageNumber,
                    Content = chunks[index].Content,
                    EmbeddingJson = JsonSerializer.Serialize(embedding),
                    EmbeddingModel = _embeddingService.GetModelName(),
                    EmbeddingDimensions = embedding.Length,
                    CreatedAt = DateTime.Now
                });
            }

            await _repository.DeleteChunksByDocumentAsync(document.DocumentId);
            await _repository.AddChunksAsync(entities);
            document.TotalChunks = entities.Count;
            document.Status = "PendingApproval";
            document.ErrorMessage = null;
            await _repository.UpdateAsync(document);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            document.Status = "Queued";
            await _repository.UpdateAsync(document);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Document processing failed for {DocumentId}.",
                document.DocumentId);
            document.Status = "Failed";
            document.ErrorMessage = SafeFailureMessage;
            await _repository.UpdateAsync(document);
        }
    }
}
