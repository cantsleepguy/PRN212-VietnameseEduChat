using PRN212_VietnameseEduChat.Services.Implementations;

namespace PRN212_VietnameseEduChat.Tests.Documents;

public sealed class DocumentProcessingQueueTests
{
    [Fact]
    public async Task Queue_dequeues_in_fifo_order()
    {
        var queue = new DocumentProcessingQueue();
        await queue.EnqueueAsync(11);
        await queue.EnqueueAsync(22);

        Assert.Equal(11, await queue.DequeueAsync(CancellationToken.None));
        Assert.Equal(22, await queue.DequeueAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Queue_coalesces_duplicate_until_dequeued()
    {
        var queue = new DocumentProcessingQueue();
        await queue.EnqueueAsync(11);
        await queue.EnqueueAsync(11);

        Assert.Equal(11, await queue.DequeueAsync(CancellationToken.None));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await queue.DequeueAsync(cancellation.Token));
    }

    [Fact]
    public async Task Empty_queue_honors_cancellation()
    {
        var queue = new DocumentProcessingQueue();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await queue.DequeueAsync(cancellation.Token));
    }
}
