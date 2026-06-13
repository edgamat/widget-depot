using WidgetDepot.Web.Features.Orders.Detail;

namespace WidgetDepot.Web.Features.Orders.History;

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

internal record RetransmitOrderApiResponse(TransmissionStatus NewStatus, DateTime StatusChangedAt);

public abstract record RetransmitResult
{
    public record Transmitted : RetransmitResult;
    public record Missing : RetransmitResult;
    public record Failed : RetransmitResult;
    public record Failure : RetransmitResult;
}

internal record RecreateOrderApiResponse(TransmissionStatus NewStatus, DateTime StatusChangedAt, string? ErrorMessage = null);

public abstract record RecreateResult
{
    public record Transmitted : RecreateResult;
    public record Failed(string ErrorMessage) : RecreateResult;
    public record Failure : RecreateResult;
}