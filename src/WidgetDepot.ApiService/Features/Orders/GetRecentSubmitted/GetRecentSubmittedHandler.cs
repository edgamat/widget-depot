using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.GetRecentSubmitted;

public record GetRecentSubmittedOrderResponse(
    int Id,
    DateTime SubmittedAt,
    int WidgetCount,
    decimal? ShippingEstimate,
    TransmissionStatus TransmissionStatus,
    DateTime? TransmissionStatusChangedAt);

public record GetRecentSubmittedQuery(int CustomerId) : IRequest<IReadOnlyList<GetRecentSubmittedOrderResponse>>;

public class GetRecentSubmittedHandler(AppDbContext db) : IRequestHandler<GetRecentSubmittedQuery, IReadOnlyList<GetRecentSubmittedOrderResponse>>
{
    public async Task<IReadOnlyList<GetRecentSubmittedOrderResponse>> HandleAsync(GetRecentSubmittedQuery query, CancellationToken cancellationToken)
    {
        var customerId = query.CustomerId;

        return await db.Orders
            .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Submitted)
            .OrderByDescending(o => o.SubmittedAt)
            .Take(10)
            .Select(o => new GetRecentSubmittedOrderResponse(
                o.Id,
                o.SubmittedAt ?? DateTime.MinValue,
                o.Items.Sum(i => i.Quantity),
                o.ShippingEstimate,
                o.TransmissionStatus ?? Data.TransmissionStatus.Pending,
                o.TransmissionStatusChangedAt))
            .ToListAsync(cancellationToken);
    }
}