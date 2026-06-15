using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.CreateDraft;

public record CreateDraftOrderItemRequest(int WidgetId, int Quantity);

public record CreateDraftOrderRequest(IReadOnlyList<CreateDraftOrderItemRequest> Items);

public record CreateDraftOrderResponse(int OrderId);

public abstract record CreateDraftOrderError
{
    public record WidgetNotFound(int WidgetId) : CreateDraftOrderError;
}

public record CreateDraftOrderCommand(int CustomerId, CreateDraftOrderRequest Request) : IRequest<object>;

public class CreateDraftOrderHandler(AppDbContext db) : IRequestHandler<CreateDraftOrderCommand, object>
{
    public async Task<object> HandleAsync(CreateDraftOrderCommand command, CancellationToken cancellationToken)
    {
        var customerId = command.CustomerId;
        var request = command.Request;

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