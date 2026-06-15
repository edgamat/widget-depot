using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.Submit;
using WidgetDepot.ApiService.Features.Orders.TransmitOrders;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.RecreateOrder;

public record RecreateOrderResponse(TransmissionStatus NewStatus, DateTime StatusChangedAt, string? ErrorMessage = null);
public record RecreateOrderNotFound;
public record RecreateOrderInvalidStatus;

public record RecreateOrderCommand(int OrderId, int CustomerId) : IRequest<object>;

public class RecreateOrderHandler : IRequestHandler<RecreateOrderCommand, object>
{
    private readonly AppDbContext _db;
    private readonly IOrderFileWriter _orderFileWriter;
    private readonly IOrderTransmitter _transmitter;
    private readonly string _pickupDirectory;
    private readonly ILogger<RecreateOrderHandler> _logger;

    public RecreateOrderHandler(
        AppDbContext db,
        IOrderFileWriter orderFileWriter,
        IOrderTransmitter transmitter,
        IOptions<OrdersOptions> options,
        ILogger<RecreateOrderHandler> logger)
    {
        _db = db;
        _orderFileWriter = orderFileWriter;
        _transmitter = transmitter;
        _pickupDirectory = options.Value.PickupDirectory;
        _logger = logger;
    }

    public async Task<object> HandleAsync(RecreateOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = command.OrderId;
        var customerId = command.CustomerId;

        var order = await _db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Widget)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId, cancellationToken);

        if (order is null)
            return new RecreateOrderNotFound();

        if (order.TransmissionStatus != TransmissionStatus.Missing)
            return new RecreateOrderInvalidStatus();

        var customer = await _db.Customers.FindAsync([customerId], cancellationToken);
        if (customer is null)
            return new RecreateOrderNotFound();

        var orderFile = new OrderFile(order, customer.Email);
        await _orderFileWriter.WriteAsync(orderFile, cancellationToken);

        var localFilePath = Path.Combine(_pickupDirectory, orderFile.FileName);

        bool success;
        string? errorMessage = null;
        try
        {
            success = await _transmitter.TransmitAsync(localFilePath, orderFile.FileName, cancellationToken);
            if (!success)
                errorMessage = "FTP transmission failed.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP transmission failed for order {OrderId}", order.Id);
            success = false;
            errorMessage = ex.Message;
        }

        order.TransmissionStatus = success ? TransmissionStatus.Transmitted : TransmissionStatus.Failed;
        order.TransmissionStatusChangedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return new RecreateOrderResponse(order.TransmissionStatus.Value, order.TransmissionStatusChangedAt.Value, errorMessage);
    }
}