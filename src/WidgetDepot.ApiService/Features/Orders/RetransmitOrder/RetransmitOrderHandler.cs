using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.Submit;
using WidgetDepot.ApiService.Features.Orders.TransmitOrders;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.RetransmitOrder;

public record RetransmitOrderResponse(TransmissionStatus NewStatus, DateTime StatusChangedAt);
public record RetransmitOrderNotFound;
public record RetransmitOrderInvalidStatus;

public record RetransmitOrderCommand(int OrderId, int CustomerId) : IRequest<object>;

public class RetransmitOrderHandler : IRequestHandler<RetransmitOrderCommand, object>
{
    private readonly AppDbContext _db;
    private readonly IOrderTransmitter _transmitter;
    private readonly string _pickupDirectory;
    private readonly ILogger<RetransmitOrderHandler> _logger;

    public RetransmitOrderHandler(
        AppDbContext db,
        IOrderTransmitter transmitter,
        IOptions<OrdersOptions> options,
        ILogger<RetransmitOrderHandler> logger)
    {
        _db = db;
        _transmitter = transmitter;
        _pickupDirectory = options.Value.PickupDirectory;
        _logger = logger;
    }

    public async Task<object> HandleAsync(RetransmitOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = command.OrderId;
        var customerId = command.CustomerId;

        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId, cancellationToken);

        if (order is null)
            return new RetransmitOrderNotFound();

        if (order.TransmissionStatus != TransmissionStatus.Failed)
            return new RetransmitOrderInvalidStatus();

        var fileName = OrderFile.GetFileName(order.Id);
        var localFilePath = Path.Combine(_pickupDirectory, fileName);

        if (!File.Exists(localFilePath))
        {
            order.TransmissionStatus = TransmissionStatus.Missing;
            order.TransmissionStatusChangedAt = DateTime.UtcNow;
            _logger.LogWarning("Order {OrderId} file not found: {FilePath}", order.Id, localFilePath);
            await _db.SaveChangesAsync(cancellationToken);
            return new RetransmitOrderResponse(order.TransmissionStatus.Value, order.TransmissionStatusChangedAt!.Value);
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

        order.TransmissionStatus = success ? TransmissionStatus.Transmitted : TransmissionStatus.Failed;
        order.TransmissionStatusChangedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return new RetransmitOrderResponse(order.TransmissionStatus.Value, order.TransmissionStatusChangedAt.Value);
    }
}