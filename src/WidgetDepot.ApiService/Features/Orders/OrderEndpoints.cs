namespace WidgetDepot.ApiService.Features.Orders;

public static class OrderEndpoints
{
    private const string Base = "orders";

    public const string CreateDraft = $"{Base}/draft";
    public const string SaveAddresses = $"{Base}/{{orderId}}/addresses";
    public const string GetDraftOrder = $"{Base}/{{orderId}}/draft";
    public const string CalculateShipping = $"{Base}/{{orderId}}/shipping-estimate";
    public const string GetDrafts = $"{Base}/drafts";
    public const string DeleteDraft = $"{Base}/{{orderId}}/draft";
    public const string SubmitOrder = $"{Base}/{{orderId}}/submit";
}