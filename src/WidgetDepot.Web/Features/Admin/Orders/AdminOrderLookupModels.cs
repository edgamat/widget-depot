using WidgetDepot.Web.Features.Orders.Detail;

namespace WidgetDepot.Web.Features.Admin.Orders;

internal record GetAdminOrderByNumberItemResponse(int WidgetId, string Sku, string Name, decimal Weight, decimal UnitCost, int Quantity);

internal record GetAdminOrderByNumberAddressResponse(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

internal record GetAdminOrderByNumberCustomerResponse(string FirstName, string LastName, string Email);

internal record GetAdminOrderByNumberResponse(
    int Id,
    string Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    IReadOnlyList<GetAdminOrderByNumberItemResponse> Items,
    GetAdminOrderByNumberAddressResponse? ShippingAddress,
    GetAdminOrderByNumberAddressResponse? BillingAddress,
    decimal? ShippingEstimate,
    TransmissionStatus? TransmissionStatus,
    DateTime? TransmissionStatusChangedAt,
    GetAdminOrderByNumberCustomerResponse? Customer);

public record AdminOrderCustomer(string FullName, string Email);

public abstract record GetAdminOrderDetailResult
{
    public record Success(OrderDetail Order, AdminOrderCustomer? Customer) : GetAdminOrderDetailResult;
    public record NotFound : GetAdminOrderDetailResult;
    public record Failure : GetAdminOrderDetailResult;
}