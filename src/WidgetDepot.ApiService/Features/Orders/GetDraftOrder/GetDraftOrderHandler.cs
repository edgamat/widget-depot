using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.GetDraftOrder;

public record GetDraftOrderItemResponse(int WidgetId, string Sku, string Name, decimal Weight, decimal UnitCost, int Quantity);

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

public abstract record GetDraftOrderError
{
    public record OrderNotFound : GetDraftOrderError;
    public record Forbidden : GetDraftOrderError;
    public record NotDraft : GetDraftOrderError;
}

public class GetDraftOrderHandler(AppDbContext db)
{
    public async Task<object> HandleAsync(int orderId, int customerId, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Widget)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return new GetDraftOrderError.OrderNotFound();

        if (order.CustomerId != customerId)
            return new GetDraftOrderError.Forbidden();

        if (order.Status != OrderStatus.Draft)
            return new GetDraftOrderError.NotDraft();

        return new GetDraftOrderResponse(
            order.Id,
            order.Status.ToString(),
            [.. order.Items.Select(i => new GetDraftOrderItemResponse(i.WidgetId, i.Widget.Sku, i.Widget.Name, i.Widget.Weight, i.Widget.Price, i.Quantity))],
            MapAddress(order.ShippingAddress),
            MapAddress(order.BillingAddress),
            order.ShippingEstimate);
    }

    private static GetDraftOrderAddressResponse? MapAddress(Address? address) =>
        address is null ? null : new GetDraftOrderAddressResponse(
            address.RecipientName,
            address.StreetLine1,
            address.StreetLine2,
            address.City,
            address.State,
            address.ZipCode);
}