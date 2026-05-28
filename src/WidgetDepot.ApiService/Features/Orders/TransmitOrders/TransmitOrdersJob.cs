namespace WidgetDepot.ApiService.Features.Orders.TransmitOrders;

public class TransmitOrdersJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TransmitOrdersJob> _logger;
    private readonly int _scheduleHourUtc;
    private bool _first = true;

    public TransmitOrdersJob(
        IServiceScopeFactory scopeFactory,
        ILogger<TransmitOrdersJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _scheduleHourUtc = configuration.GetValue("TransmissionJob:ScheduleHourUtc", 17);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNextScheduledRun();

            try
            {
                _logger.LogInformation("TransmitOrdersJob: Delaying for {Delay}", delay);
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
        var handler = scope.ServiceProvider.GetRequiredService<TransmitOrdersHandler>();

        var processed = await handler.HandleAsync(cancellationToken);
        _logger.LogInformation("TransmitOrdersJob: processed {Count} order(s).", processed);
    }

    private TimeSpan TimeUntilNextScheduledRun()
    {
        if (_first)
        {
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