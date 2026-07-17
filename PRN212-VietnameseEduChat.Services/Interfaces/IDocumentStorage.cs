namespace PRN212_VietnameseEduChat.Services.Interfaces;

public sealed record StoredDocumentFile(string StoredFileName);

public interface IDocumentStorage
{
    Task<StoredDocumentFile> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        string storedFileName,
        CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(
        string storedFileName,
        CancellationToken cancellationToken = default);
}
