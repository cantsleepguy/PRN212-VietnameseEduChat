using System.Threading.Channels;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Services.Implementations;

public sealed class DocumentProcessingQueue : IDocumentProcessingQueue
{
    private readonly Channel<int> _channel = Channel.CreateBounded<int>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    private readonly HashSet<int> _queued = [];
    private readonly object _sync = new();

    public async ValueTask EnqueueAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (!_queued.Add(documentId))
            {
                return;
            }
        }

        try
        {
            await _channel.Writer.WriteAsync(documentId, cancellationToken);
        }
        catch
        {
            lock (_sync)
            {
                _queued.Remove(documentId);
            }

            throw;
        }
    }

    public async ValueTask<int> DequeueAsync(CancellationToken cancellationToken)
    {
        var documentId = await _channel.Reader.ReadAsync(cancellationToken);
        lock (_sync)
        {
            _queued.Remove(documentId);
        }

        return documentId;
    }
}
