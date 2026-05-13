using System.ComponentModel.DataAnnotations;

namespace WidgetDepot.Web.Features.Orders.Detail;

public record OrderDetailItem(int WidgetId, string Sku, string Name, decimal Weight, decimal UnitCost, int Quantity);

public record OrderDetailAddress(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record OrderDetail(
    int Id,
    string Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    IReadOnlyList<OrderDetailItem> Items,
    OrderDetailAddress? ShippingAddress,
    OrderDetailAddress? BillingAddress,
    decimal? ShippingEstimate);

public abstract record GetOrderDetailResult
{
    public record Success(OrderDetail Order) : GetOrderDetailResult;
    public record NotFound : GetOrderDetailResult;
    public record Failure : GetOrderDetailResult;
}

public class LookupFormModel
{
    [Required(ErrorMessage = "Order number is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Order number must be a positive number.")]
    public int? OrderNumber { get; set; }
}

internal record GetByOrderNumberItemResponse(int WidgetId, string Sku, string Name, decimal Weight, decimal UnitCost, int Quantity);

internal record GetByOrderNumberAddressResponse(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

internal record GetByOrderNumberResponse(
    int Id,
    string Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    IReadOnlyList<GetByOrderNumberItemResponse> Items,
    GetByOrderNumberAddressResponse? ShippingAddress,
    GetByOrderNumberAddressResponse? BillingAddress,
    decimal? ShippingEstimate);