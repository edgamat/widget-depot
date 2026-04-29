using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.GetDraftOrder;

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

public abstract record GetDraftOrderError
{
    public record OrderNotFound : GetDraftOrderError;
    public record Forbidden : GetDraftOrderError;
}

public class GetDraftOrderHandler(AppDbContext db)
{
    public async Task<object> HandleAsync(int orderId, int customerId, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return new GetDraftOrderError.OrderNotFound();

        if (order.CustomerId != customerId)
            return new GetDraftOrderError.Forbidden();

        return new GetDraftOrderResponse(
            order.Id,
            order.Status.ToString(),
            [.. order.Items.Select(i => new GetDraftOrderItemResponse(i.WidgetId, i.Quantity))],
            MapAddress(order.ShippingAddress),
            MapAddress(order.BillingAddress));
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
