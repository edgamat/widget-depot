using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.DeleteDraft;

public abstract record DeleteDraftError
{
    public record OrderNotFound : DeleteDraftError;
    public record Forbidden : DeleteDraftError;
}

public record DeleteDraftCommand(int OrderId, int CustomerId) : IRequest<DeleteDraftError?>;

public class DeleteDraftHandler(AppDbContext db) : IRequestHandler<DeleteDraftCommand, DeleteDraftError?>
{
    public async Task<DeleteDraftError?> HandleAsync(DeleteDraftCommand command, CancellationToken cancellationToken)
    {
        var orderId = command.OrderId;
        var customerId = command.CustomerId;

        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return new DeleteDraftError.OrderNotFound();

        if (order.CustomerId != customerId)
            return new DeleteDraftError.Forbidden();

        db.Orders.Remove(order);
        await db.SaveChangesAsync(cancellationToken);

        return null;
    }
}