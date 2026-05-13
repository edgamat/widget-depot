using System.Net;
using System.Net.Http.Json;

namespace WidgetDepot.Web.Features.Orders.Detail;

public class DetailService
{
    private readonly HttpClient _httpClient;

    public DetailService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetOrderDetailResult> GetOrderDetailAsync(int orderNumber, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/orders/{orderNumber}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new GetOrderDetailResult.NotFound();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var dto = await response.Content.ReadFromJsonAsync<GetByOrderNumberResponse>(cancellationToken);
            if (dto is null)
                return new GetOrderDetailResult.Failure();

            var order = new OrderDetail(
                dto.Id,
                dto.Status,
                dto.CreatedAt,
                dto.SubmittedAt,
                [.. dto.Items.Select(i => new OrderDetailItem(i.WidgetId, i.Sku, i.Name, i.Weight, i.UnitCost, i.Quantity))],
                MapAddress(dto.ShippingAddress),
                MapAddress(dto.BillingAddress),
                dto.ShippingEstimate);

            return new GetOrderDetailResult.Success(order);
        }

        return new GetOrderDetailResult.Failure();
    }

    private static OrderDetailAddress? MapAddress(GetByOrderNumberAddressResponse? address) =>
        address is null ? null : new OrderDetailAddress(
            address.RecipientName,
            address.StreetLine1,
            address.StreetLine2,
            address.City,
            address.State,
            address.ZipCode);
}