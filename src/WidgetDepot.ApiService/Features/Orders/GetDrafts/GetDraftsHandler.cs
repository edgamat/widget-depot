using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.GetDrafts;

public record GetDraftsOrderResponse(int Id, int WidgetCount, DateTime CreatedAt);

public record GetDraftsQuery(int CustomerId) : IRequest<IReadOnlyList<GetDraftsOrderResponse>>;

public class GetDraftsHandler(AppDbContext db) : IRequestHandler<GetDraftsQuery, IReadOnlyList<GetDraftsOrderResponse>>
{
    public async Task<IReadOnlyList<GetDraftsOrderResponse>> HandleAsync(GetDraftsQuery query, CancellationToken cancellationToken)
    {
        var customerId = query.CustomerId;

        return await db.Orders
            .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Draft)
            .Select(o => new GetDraftsOrderResponse(
                o.Id,
                o.Items.Sum(i => i.Quantity),
                o.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}