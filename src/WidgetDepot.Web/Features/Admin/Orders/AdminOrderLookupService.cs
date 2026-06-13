using System.Net;

using WidgetDepot.Web.Features.Orders.Detail;

namespace WidgetDepot.Web.Features.Admin.Orders;

public class AdminOrderLookupService
{
    private readonly HttpClient _httpClient;

    public AdminOrderLookupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetAdminOrderDetailResult> GetOrderDetailAsync(int orderNumber, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/admin/orders/{orderNumber}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new GetAdminOrderDetailResult.NotFound();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var dto = await response.Content.ReadFromJsonAsync<GetAdminOrderByNumberResponse>(cancellationToken);
            if (dto is null)
                return new GetAdminOrderDetailResult.Failure();

            var order = new OrderDetail(
                dto.Id,
                dto.Status,
                dto.CreatedAt,
                dto.SubmittedAt,
                [.. dto.Items.Select(i => new OrderDetailItem(i.WidgetId, i.Sku, i.Name, i.Weight, i.UnitCost, i.Quantity))],
                MapAddress(dto.ShippingAddress),
                MapAddress(dto.BillingAddress),
                dto.ShippingEstimate,
                dto.TransmissionStatus,
                dto.TransmissionStatusChangedAt);

            AdminOrderCustomer? customer = dto.Customer is null
                ? null
                : new AdminOrderCustomer($"{dto.Customer.FirstName} {dto.Customer.LastName}", dto.Customer.Email);

            return new GetAdminOrderDetailResult.Success(order, customer);
        }

        return new GetAdminOrderDetailResult.Failure();
    }

    private static OrderDetailAddress? MapAddress(GetAdminOrderByNumberAddressResponse? address) =>
        address is null ? null : new OrderDetailAddress(
            address.RecipientName,
            address.StreetLine1,
            address.StreetLine2,
            address.City,
            address.State,
            address.ZipCode);
}