using System.Net;
using System.Net.Http.Json;

namespace WidgetDepot.Web.Features.Orders.History;

public class HistoryService
{
    private readonly HttpClient _httpClient;

    public HistoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetRecentOrdersResult> GetRecentOrdersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/orders/recent", cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var orders = await response.Content.ReadFromJsonAsync<List<GetRecentSubmittedResponse>>(cancellationToken);
            if (orders is null)
                return new GetRecentOrdersResult.Failure();

            var items = orders
                .Select(o => new RecentOrderListItem(o.Id, o.SubmittedAt, o.WidgetCount, o.ShippingEstimate))
                .ToList();

            return new GetRecentOrdersResult.Success(items);
        }

        return new GetRecentOrdersResult.Failure();
    }
}