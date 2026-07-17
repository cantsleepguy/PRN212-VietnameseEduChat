using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.HostedServices;

public sealed class DocumentProcessingWorker : BackgroundService
{
    private readonly IDocumentProcessingQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentProcessingWorker> _logger;

    public DocumentProcessingWorker(
        IDocumentProcessingQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<DocumentProcessingWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var documentId = await _queue.DequeueAsync(stoppingToken);
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<IDocumentProcessor>();
                await processor.ProcessAsync(documentId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Document background worker encountered an unexpected error.");
            }
        }
    }
}
