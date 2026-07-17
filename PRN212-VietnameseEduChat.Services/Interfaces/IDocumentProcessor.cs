namespace PRN212_VietnameseEduChat.Services.Interfaces;

public interface IDocumentProcessor
{
    Task ProcessAsync(int documentId, CancellationToken cancellationToken = default);
}
