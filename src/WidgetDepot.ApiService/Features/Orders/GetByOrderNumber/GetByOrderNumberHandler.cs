using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.GetByOrderNumber;

public record GetByOrderNumberItemResponse(int WidgetId, string Sku, string Name, decimal Weight, decimal UnitCost, int Quantity);

public record GetByOrderNumberAddressResponse(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record GetByOrderNumberResponse(
    int Id,
    string Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    IReadOnlyList<GetByOrderNumberItemResponse> Items,
    GetByOrderNumberAddressResponse? ShippingAddress,
    GetByOrderNumberAddressResponse? BillingAddress,
    decimal? ShippingEstimate);

public record GetByOrderNumberNotFound;

public class GetByOrderNumberHandler(AppDbContext db)
{
    public async Task<object> HandleAsync(int orderNumber, int customerId, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Widget)
            .FirstOrDefaultAsync(o => o.Id == orderNumber && o.CustomerId == customerId, cancellationToken);

        if (order is null)
            return new GetByOrderNumberNotFound();

        return new GetByOrderNumberResponse(
            order.Id,
            order.Status.ToString(),
            order.CreatedAt,
            order.SubmittedAt,
            [.. order.Items.Select(i => new GetByOrderNumberItemResponse(i.WidgetId, i.Widget.Sku, i.Widget.Name, i.Widget.Weight, i.Widget.Price, i.Quantity))],
            MapAddress(order.ShippingAddress),
            MapAddress(order.BillingAddress),
            order.ShippingEstimate);
    }

    private static GetByOrderNumberAddressResponse? MapAddress(Address? address) =>
        address is null ? null : new GetByOrderNumberAddressResponse(
            address.RecipientName,
            address.StreetLine1,
            address.StreetLine2,
            address.City,
            address.State,
            address.ZipCode);
}