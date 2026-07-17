using Microsoft.Extensions.Logging.Abstractions;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Documents;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Implementations;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Tests.Documents;

public sealed class DocumentProcessorTests : IDisposable
{
    private readonly string _path = Path.GetTempFileName();

    [Fact]
    public async Task Process_moves_queued_document_to_pending_approval()
    {
        var repository = new DocumentRepositoryFake(new Document
        {
            DocumentId = 1,
            StoredFileName = "lesson.pdf",
            Status = "Queued"
        });
        var processor = CreateProcessor(repository, new TextExtractorFake("lesson content"));

        await processor.ProcessAsync(1);

        Assert.Equal("PendingApproval", repository.Document.Status);
        Assert.Equal(1, repository.Document.TotalChunks);
        Assert.Single(repository.Chunks);
    }

    [Fact]
    public async Task Process_stores_safe_message_when_extraction_fails()
    {
        var repository = new DocumentRepositoryFake(new Document
        {
            DocumentId = 1,
            StoredFileName = "lesson.pdf",
            Status = "Queued"
        });
        var processor = CreateProcessor(
            repository,
            new TextExtractorFake(exception: new InvalidOperationException("C:\\secret\\source.pdf API_KEY")));

        await processor.ProcessAsync(1);

        Assert.Equal("Failed", repository.Document.Status);
        Assert.Equal(
            "Không thể xử lý tài liệu. Vui lòng kiểm tra file và thử lại.",
            repository.Document.ErrorMessage);
        Assert.DoesNotContain("secret", repository.Document.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose() => File.Delete(_path);

    private DocumentProcessor CreateProcessor(
        IDocumentRepository repository,
        ITextExtractorService extractor) =>
        new(
            repository,
            new StorageFake(_path),
            extractor,
            new ChunkServiceFake(),
            new EmbeddingServiceFake(),
            new ConfigurationServiceFake(),
            NullLogger<DocumentProcessor>.Instance);

    private sealed class DocumentRepositoryFake(Document document) : IDocumentRepository
    {
        public Document Document { get; } = document;
        public List<DocumentChunk> Chunks { get; } = [];
        public Task<Document?> GetByIdAsync(int id) => Task.FromResult<Document?>(id == Document.DocumentId ? Document : null);
        public Task UpdateAsync(Document value) => Task.CompletedTask;
        public Task AddChunksAsync(List<DocumentChunk> chunks) { Chunks.AddRange(chunks); return Task.CompletedTask; }
        public Task DeleteChunksByDocumentAsync(int documentId) { Chunks.Clear(); return Task.CompletedTask; }
        public Task<List<Document>> GetAllAsync() => throw new NotSupportedException();
        public Task<Document?> GetByIdWithChunksAsync(int id) => throw new NotSupportedException();
        public Task AddAsync(Document value) => throw new NotSupportedException();
        public Task DeleteAsync(Document value) => throw new NotSupportedException();
        public Task<List<Document>> GetPendingProcessingAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class StorageFake(string path) : IDocumentStorage
    {
        public string GetPhysicalPath(string storedFileName) => path;
        public Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<StoredDocumentFile> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteIfExistsAsync(string storedFileName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class TextExtractorFake(string? text = null, Exception? exception = null) : ITextExtractorService
    {
        public Task<string> ExtractAsync(string filePath, string extension) =>
            exception == null ? Task.FromResult(text ?? string.Empty) : Task.FromException<string>(exception);
    }

    private sealed class ChunkServiceFake : IChunkService
    {
        public List<string> Chunk(string text) => [text];
        public List<ChunkResultDto> Chunk(string text, ChunkingConfiguration configuration) => [new() { Content = text }];
    }

    private sealed class EmbeddingServiceFake : IEmbeddingService
    {
        public Task<float[]> CreateEmbeddingAsync(string text, string? modelName = null, int? dimensions = null) => Task.FromResult(new[] { 0.1f, 0.2f });
        public string GetModelName() => "test-embedding";
        public int GetDimensions(string? modelName = null) => 2;
    }

    private sealed class ConfigurationServiceFake : IChunkingConfigurationService
    {
        public Task<ChunkingConfiguration> GetActiveAsync() => Task.FromResult(new ChunkingConfiguration());
        public Task<List<ChunkingConfiguration>> GetAllAsync() => throw new NotSupportedException();
        public Task ActivateAsync(int configurationId, int userId) => throw new NotSupportedException();
        public Task UpdateAsync(int configurationId, int chunkSize, int chunkOverlap, string fixedSizeUnit, int userId) => throw new NotSupportedException();
        public Task EnsureDefaultsAsync() => throw new NotSupportedException();
    }
}
