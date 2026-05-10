using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.Submit;

public record SubmitOrderResponse(int OrderId);

public abstract record SubmitOrderError
{
    public record OrderNotFound : SubmitOrderError;
    public record Forbidden : SubmitOrderError;
    public record InvalidOrderState : SubmitOrderError;
    public record IncompleteOrder(string Reason) : SubmitOrderError;
}

public class SubmitOrderHandler
{
    private readonly AppDbContext _db;
    private readonly IOrderFileWriter _orderFileWriter;

    public SubmitOrderHandler(AppDbContext db, IOrderFileWriter orderFileWriter)
    {
        _db = db;
        _orderFileWriter = orderFileWriter;
    }

    public async Task<object> HandleAsync(int orderId, int customerId, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Widget)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return new SubmitOrderError.OrderNotFound();

        if (order.CustomerId != customerId)
            return new SubmitOrderError.Forbidden();

        if (order.Status != OrderStatus.Draft)
            return new SubmitOrderError.InvalidOrderState();

        if (!order.Items.Any())
            return new SubmitOrderError.IncompleteOrder("Order has no items.");

        if (order.ShippingAddress is null)
            return new SubmitOrderError.IncompleteOrder("Shipping address is missing.");

        if (order.BillingAddress is null)
            return new SubmitOrderError.IncompleteOrder("Billing address is missing.");

        if (order.ShippingEstimate is null)
            return new SubmitOrderError.IncompleteOrder("Shipping estimate is missing.");

        var customer = await _db.Customers.FindAsync([customerId], cancellationToken);
        if (customer is null)
            return new SubmitOrderError.OrderNotFound();

        order.Status = OrderStatus.Submitted;
        order.SubmittedAt = DateTime.UtcNow;

        var orderFile = new OrderFile(order, customer.Email);
        await _orderFileWriter.WriteAsync(orderFile, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        return new SubmitOrderResponse(order.Id);
    }
}