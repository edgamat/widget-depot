namespace WidgetDepot.Web.Features.Orders.History;

public enum TransmissionStatus
{
    Pending = 0,
    Transmitted = 1,
    Failed = 2,
    Missing = 3
}

public record RecentOrderListItem(
    int Id,
    DateTime SubmittedAt,
    int WidgetCount,
    decimal? ShippingEstimate,
    TransmissionStatus TransmissionStatus,
    DateTime? TransmissionStatusChangedAt);

public abstract record GetRecentOrdersResult
{
    public record Success(IReadOnlyList<RecentOrderListItem> Orders) : GetRecentOrdersResult;
    public record Failure : GetRecentOrdersResult;
}

internal record GetRecentSubmittedResponse(
    int Id,
    DateTime SubmittedAt,
    int WidgetCount,
    decimal? ShippingEstimate,
    TransmissionStatus TransmissionStatus,
    DateTime? TransmissionStatusChangedAt);