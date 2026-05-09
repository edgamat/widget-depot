using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.GetDrafts;

public record GetDraftsOrderResponse(int Id, int WidgetCount, DateTime CreatedAt);

public class GetDraftsHandler(AppDbContext db)
{
    public async Task<IReadOnlyList<GetDraftsOrderResponse>> HandleAsync(int customerId, CancellationToken cancellationToken)
    {
        return await db.Orders
            .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Draft)
            .Select(o => new GetDraftsOrderResponse(
                o.Id,
                o.Items.Sum(i => i.Quantity),
                o.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}