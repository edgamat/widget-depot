namespace WidgetDepot.Web.Features.Orders.History;

public record RecentOrderListItem(int Id, DateTime SubmittedAt, int WidgetCount, decimal? ShippingEstimate);

public abstract record GetRecentOrdersResult
{
    public record Success(IReadOnlyList<RecentOrderListItem> Orders) : GetRecentOrdersResult;
    public record Failure : GetRecentOrdersResult;
}

internal record GetRecentSubmittedResponse(int Id, DateTime SubmittedAt, int WidgetCount, decimal? ShippingEstimate);