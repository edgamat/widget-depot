namespace WidgetDepot.ApiService.Features.Orders.ExpireDraftOrders;

public class ExpireDraftOrdersJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpireDraftOrdersJob> _logger;
    private readonly int _scheduleHourUtc;
    private bool _first = true;

    public ExpireDraftOrdersJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpireDraftOrdersJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _scheduleHourUtc = configuration.GetValue("DraftOrderExpiry:ScheduleHourUtc", 0);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNextScheduledRun();

            try
            {
                _logger.LogInformation("ExpireDraftOrdersJob: Delaying for {Delay}", delay);
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RunJobAsync(stoppingToken);
        }
    }

    private async Task RunJobAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ExpireDraftOrdersHandler>();

        var deleted = await handler.HandleAsync(cancellationToken);
        _logger.LogInformation("ExpireDraftOrdersJob: deleted {Count} expired draft order(s).", deleted);
    }

    private TimeSpan TimeUntilNextScheduledRun()
    {
        if (_first)
        {
            // Run the job on startup
            _first = false;
            return TimeSpan.Zero;
        }

        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddHours(_scheduleHourUtc);

        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }
}