using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.CreateDraft;

public record CreateDraftOrderItemRequest(int WidgetId, int Quantity);

public record CreateDraftOrderRequest(IReadOnlyList<CreateDraftOrderItemRequest> Items);

public record CreateDraftOrderResponse(int OrderId);

public abstract record CreateDraftOrderError
{
    public record WidgetNotFound(int WidgetId) : CreateDraftOrderError;
}

public class CreateDraftOrderHandler(AppDbContext db)
{
    public async Task<object> HandleAsync(int customerId, CreateDraftOrderRequest request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            var widget = await db.Widgets
                .FirstOrDefaultAsync(w => w.Id == item.WidgetId, cancellationToken);

            if (widget is null)
                return new CreateDraftOrderError.WidgetNotFound(item.WidgetId);

            order.Items.Add(new OrderItem
            {
                WidgetId = item.WidgetId,
                Quantity = item.Quantity
            });
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateDraftOrderResponse(order.Id);
    }
}