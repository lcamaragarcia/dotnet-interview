namespace TodoApi.Synchronization;

public class SynchronizationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SynchronizationBackgroundService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);

    public SynchronizationBackgroundService(IServiceProvider services, ILogger<SynchronizationBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de sincronización en segundo plano iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<ISynchronizationService>();
                    await syncService.SyncAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en el servicio de sincronización. Reintentando en {Interval}...", _syncInterval);
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }
    }
}
