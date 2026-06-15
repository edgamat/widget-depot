using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Orders.GetAdminOrderByNumber;

public record GetAdminOrderByNumberItemResponse(int WidgetId, string Sku, string Name, decimal Weight, decimal UnitCost, int Quantity);

public record GetAdminOrderByNumberAddressResponse(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record GetAdminOrderByNumberCustomerResponse(string FirstName, string LastName, string Email);

public record GetAdminOrderByNumberResponse(
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

public record GetAdminOrderByNumberNotFound;

public record GetAdminOrderByNumberQuery(int OrderNumber) : IRequest<object>;

public class GetAdminOrderByNumberHandler(AppDbContext db) : IRequestHandler<GetAdminOrderByNumberQuery, object>
{
    public async Task<object> HandleAsync(GetAdminOrderByNumberQuery query, CancellationToken cancellationToken)
    {
        var orderNumber = query.OrderNumber;

        var order = await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Widget)
            .FirstOrDefaultAsync(o => o.Id == orderNumber, cancellationToken);

        if (order is null)
            return new GetAdminOrderByNumberNotFound();

        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);

        return new GetAdminOrderByNumberResponse(
            order.Id,
            order.Status.ToString(),
            order.CreatedAt,
            order.SubmittedAt,
            [.. order.Items.Select(i => new GetAdminOrderByNumberItemResponse(i.WidgetId, i.Widget.Sku, i.Widget.Name, i.Widget.Weight, i.Widget.Price, i.Quantity))],
            MapAddress(order.ShippingAddress),
            MapAddress(order.BillingAddress),
            order.ShippingEstimate,
            order.TransmissionStatus,
            order.TransmissionStatusChangedAt,
            customer is null ? null : new GetAdminOrderByNumberCustomerResponse(customer.FirstName, customer.LastName, customer.Email));
    }

    private static GetAdminOrderByNumberAddressResponse? MapAddress(Address? address) =>
        address is null ? null : new GetAdminOrderByNumberAddressResponse(
            address.RecipientName,
            address.StreetLine1,
            address.StreetLine2,
            address.City,
            address.State,
            address.ZipCode);
}