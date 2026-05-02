namespace WidgetDepot.Web.Features.Orders.Create.Step3;

public abstract record GetDraftOrderResult
{
    public record Success(GetDraftOrderResponse Order) : GetDraftOrderResult;
    public record NotFound : GetDraftOrderResult;
    public record Forbidden : GetDraftOrderResult;
    public record Failure : GetDraftOrderResult;
}

public abstract record CalculateShippingResult
{
    public record Success(decimal EstimatedCost, string Currency) : CalculateShippingResult;
    public record NotFound : CalculateShippingResult;
    public record Forbidden : CalculateShippingResult;
    public record NoShippingAddress : CalculateShippingResult;
    public record ApiFailure : CalculateShippingResult;
    public record Failure : CalculateShippingResult;
}

public record GetDraftOrderItemResponse(int WidgetId, int Quantity);

public record GetDraftOrderAddressResponse(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record GetDraftOrderResponse(
    int Id,
    string Status,
    IReadOnlyList<GetDraftOrderItemResponse> Items,
    GetDraftOrderAddressResponse? ShippingAddress,
    GetDraftOrderAddressResponse? BillingAddress);

internal record CalculateShippingResponse(decimal EstimatedCost, string Currency);