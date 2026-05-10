namespace WidgetDepot.Web.Features.Orders.Submit;

public abstract record GetDraftOrderResult
{
    public record Success(GetDraftOrderResponse Order) : GetDraftOrderResult;
    public record NotFound : GetDraftOrderResult;
    public record Forbidden : GetDraftOrderResult;
    public record Failure : GetDraftOrderResult;
}

public abstract record SubmitOrderResult
{
    public record Success(int OrderId) : SubmitOrderResult;
    public record NotFound : SubmitOrderResult;
    public record Forbidden : SubmitOrderResult;
    public record InvalidState : SubmitOrderResult;
    public record IncompleteOrder(string Reason) : SubmitOrderResult;
    public record Failure : SubmitOrderResult;
}

public record GetDraftOrderItemResponse(int WidgetId, string Sku, string Name, decimal Weight, int Quantity);

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
    GetDraftOrderAddressResponse? BillingAddress,
    decimal? ShippingEstimate);

internal record SubmitOrderApiResponse(int OrderId);