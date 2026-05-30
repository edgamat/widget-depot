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

    public async Task<RetransmitResult> RetransmitOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"/orders/{orderId}/retransmit", null, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<RetransmitOrderApiResponse>(cancellationToken);
            if (result is null)
                return new RetransmitResult.Failure();

            return result.NewStatus switch
            {
                TransmissionStatus.Transmitted => new RetransmitResult.Transmitted(),
                TransmissionStatus.Missing => new RetransmitResult.Missing(),
                TransmissionStatus.Failed => new RetransmitResult.Failed(),
                _ => new RetransmitResult.Failure()
            };
        }

        return new RetransmitResult.Failure();
    }

    public async Task<RecreateResult> RecreateOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"/orders/{orderId}/recreate", null, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<RecreateOrderApiResponse>(cancellationToken);
            if (result is null)
                return new RecreateResult.Failure();

            return result.NewStatus switch
            {
                TransmissionStatus.Transmitted => new RecreateResult.Transmitted(),
                TransmissionStatus.Failed => new RecreateResult.Failed(result.ErrorMessage ?? "FTP transmission failed."),
                _ => new RecreateResult.Failure()
            };
        }

        return new RecreateResult.Failure();
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
                .Select(o => new RecentOrderListItem(
                    o.Id,
                    o.SubmittedAt,
                    o.WidgetCount,
                    o.ShippingEstimate,
                    o.TransmissionStatus,
                    o.TransmissionStatusChangedAt))
                .ToList();

            return new GetRecentOrdersResult.Success(items);
        }

        return new GetRecentOrdersResult.Failure();
    }
}