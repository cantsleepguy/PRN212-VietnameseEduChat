namespace PRN212_VietnameseEduChat.Services.Interfaces;

public interface IDocumentProcessingQueue
{
    ValueTask EnqueueAsync(
        int documentId,
        CancellationToken cancellationToken = default);

    ValueTask<int> DequeueAsync(CancellationToken cancellationToken);
}
