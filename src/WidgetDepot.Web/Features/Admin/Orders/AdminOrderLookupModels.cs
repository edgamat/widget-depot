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
    DateTime? TransmissionStatusChangedAt);