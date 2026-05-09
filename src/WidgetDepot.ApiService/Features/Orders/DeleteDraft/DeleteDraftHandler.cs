using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.DeleteDraft;

public abstract record DeleteDraftError
{
    public record OrderNotFound : DeleteDraftError;
    public record Forbidden : DeleteDraftError;
}

public class DeleteDraftHandler(AppDbContext db)
{
    public async Task<DeleteDraftError?> HandleAsync(int orderId, int customerId, CancellationToken cancellationToken)
    {
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