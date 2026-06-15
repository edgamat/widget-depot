using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.UpdateDraftItems;

public record UpdateDraftItemRequest(int WidgetId, int Quantity);

public record UpdateDraftItemsRequest(IReadOnlyList<UpdateDraftItemRequest> Items);

public abstract record UpdateDraftItemsError
{
    public record OrderNotFound : UpdateDraftItemsError;
    public record Forbidden : UpdateDraftItemsError;
    public record NotDraft : UpdateDraftItemsError;
    public record WidgetNotFound(int WidgetId) : UpdateDraftItemsError;
}

public record UpdateDraftItemsCommand(int OrderId, int CustomerId, UpdateDraftItemsRequest Request) : IRequest<object?>;

public class UpdateDraftItemsHandler(AppDbContext db) : IRequestHandler<UpdateDraftItemsCommand, object?>
{
    public async Task<object?> HandleAsync(UpdateDraftItemsCommand command, CancellationToken cancellationToken)
    {
        var orderId = command.OrderId;
        var customerId = command.CustomerId;
        var request = command.Request;

        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return new UpdateDraftItemsError.OrderNotFound();

        if (order.CustomerId != customerId)
            return new UpdateDraftItemsError.Forbidden();

        if (order.Status != OrderStatus.Draft)
            return new UpdateDraftItemsError.NotDraft();

        order.Items.Clear();

        foreach (var item in request.Items)
        {
            var widget = await db.Widgets
                .FirstOrDefaultAsync(w => w.Id == item.WidgetId, cancellationToken);

            if (widget is null)
                return new UpdateDraftItemsError.WidgetNotFound(item.WidgetId);

            order.Items.Add(new OrderItem
            {
                WidgetId = item.WidgetId,
                Quantity = item.Quantity
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return null;
    }
}