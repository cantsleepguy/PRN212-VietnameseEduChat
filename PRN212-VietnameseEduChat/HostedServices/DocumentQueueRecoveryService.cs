using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.HostedServices;

public sealed class DocumentQueueRecoveryService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDocumentProcessingQueue _queue;

    public DocumentQueueRecoveryService(
        IServiceScopeFactory scopeFactory,
        IDocumentProcessingQueue queue)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var documents = await repository.GetPendingProcessingAsync(cancellationToken);
        foreach (var document in documents)
        {
            if (document.Status == "Processing")
            {
                document.Status = "Queued";
                await repository.UpdateAsync(document);
            }

            await _queue.EnqueueAsync(document.DocumentId, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
