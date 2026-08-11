using System.Threading.Channels;

namespace SistemaDeCompras.Services;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue =
        Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>();

    public void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        _queue.Writer.TryWrite(workItem);
    }

    public IAsyncEnumerable<Func<IServiceProvider, CancellationToken, Task>> DequeueAllAsync(CancellationToken ct)
        => _queue.Reader.ReadAllAsync(ct);
}
