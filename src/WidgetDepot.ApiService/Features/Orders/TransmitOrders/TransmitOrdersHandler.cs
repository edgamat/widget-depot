using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.Submit;

namespace WidgetDepot.ApiService.Features.Orders.TransmitOrders;

public class TransmitOrdersHandler
{
    private readonly AppDbContext _db;
    private readonly IOrderTransmitter _transmitter;
    private readonly string _pickupDirectory;
    private readonly ILogger<TransmitOrdersHandler> _logger;

    public TransmitOrdersHandler(
        AppDbContext db,
        IOrderTransmitter transmitter,
        IConfiguration configuration,
        ILogger<TransmitOrdersHandler> logger)
    {
        _db = db;
        _transmitter = transmitter;
        _pickupDirectory = configuration["Orders:PickupDirectory"] ?? "/tmp/orders";
        _logger = logger;
    }

    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        var orders = await _db.Orders
            .Where(o => o.TransmissionStatus == TransmissionStatus.Pending ||
                        o.TransmissionStatus == TransmissionStatus.Missing)
            .ToListAsync(cancellationToken);

        var processed = 0;

        foreach (var order in orders)
        {
            var fileName = OrderFile.GetFileName(order.Id);
            var localFilePath = Path.Combine(_pickupDirectory, fileName);

            if (!File.Exists(localFilePath))
            {
                order.TransmissionStatus = TransmissionStatus.Missing;
                order.TransmissionStatusChangedAt = DateTime.UtcNow;
                _logger.LogWarning("Order {OrderId} file not found: {FilePath}", order.Id, localFilePath);
                processed++;
                continue;
            }

            bool success;
            try
            {
                success = await _transmitter.TransmitAsync(localFilePath, fileName, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FTP transmission failed for order {OrderId}", order.Id);
                success = false;
            }

            order.TransmissionStatus = success
                ? TransmissionStatus.Transmitted
                : TransmissionStatus.Failed;
            order.TransmissionStatusChangedAt = DateTime.UtcNow;
            processed++;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return processed;
    }
}