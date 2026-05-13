using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.GetRecentSubmitted;

public record GetRecentSubmittedOrderResponse(int Id, DateTime SubmittedAt, int WidgetCount, decimal? ShippingEstimate);

public class GetRecentSubmittedHandler(AppDbContext db)
{
    public async Task<IReadOnlyList<GetRecentSubmittedOrderResponse>> HandleAsync(int customerId, CancellationToken cancellationToken)
    {
        return await db.Orders
            .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Submitted)
            .OrderByDescending(o => o.SubmittedAt)
            .Take(10)
            .Select(o => new GetRecentSubmittedOrderResponse(
                o.Id,
                o.SubmittedAt ?? DateTime.MinValue,
                o.Items.Sum(i => i.Quantity),
                o.ShippingEstimate))
            .ToListAsync(cancellationToken);
    }
}