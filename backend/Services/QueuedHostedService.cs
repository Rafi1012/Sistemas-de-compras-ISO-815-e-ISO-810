namespace SistemaDeCompras.Services;

public class QueuedHostedService : BackgroundService
{
    private readonly BackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(BackgroundTaskQueue taskQueue, IServiceProvider serviceProvider, ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in _taskQueue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await workItem(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando una tarea en segundo plano.");
            }
        }
    }
}
