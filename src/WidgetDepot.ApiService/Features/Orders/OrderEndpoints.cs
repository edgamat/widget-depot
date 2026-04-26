namespace WidgetDepot.ApiService.Features.Orders;

public static class OrderEndpoints
{
    private const string Base = "orders";

    public const string CreateDraft = $"{Base}/draft";
    public const string SaveAddresses = $"{Base}/{{orderId}}/addresses";
}