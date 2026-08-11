namespace SistemaDeCompras.Services;

public interface IBackgroundTaskQueue
{
    void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem);
}
