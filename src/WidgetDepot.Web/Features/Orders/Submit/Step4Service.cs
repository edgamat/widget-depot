using System.Net;
using System.Net.Http.Json;

namespace WidgetDepot.Web.Features.Orders.Submit;

public class Step4Service
{
    private readonly HttpClient _httpClient;

    public Step4Service(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetDraftOrderResult> GetDraftOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/orders/{orderId}/draft", cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var order = await response.Content.ReadFromJsonAsync<GetDraftOrderResponse>(cancellationToken);
            return order is null
                ? new GetDraftOrderResult.Failure()
                : new GetDraftOrderResult.Success(order);
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => new GetDraftOrderResult.NotFound(),
            HttpStatusCode.Forbidden => new GetDraftOrderResult.Forbidden(),
            _ => new GetDraftOrderResult.Failure()
        };
    }

    public async Task<SubmitOrderResult> SubmitOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"/orders/{orderId}/submit", null, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<SubmitOrderApiResponse>(cancellationToken);
            return result is null
                ? new SubmitOrderResult.Failure()
                : new SubmitOrderResult.Success(result.OrderId);
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => new SubmitOrderResult.NotFound(),
            HttpStatusCode.Forbidden => new SubmitOrderResult.Forbidden(),
            HttpStatusCode.Conflict => new SubmitOrderResult.InvalidState(),
            HttpStatusCode.UnprocessableEntity => new SubmitOrderResult.IncompleteOrder("Order is missing required information."),
            _ => new SubmitOrderResult.Failure()
        };
    }
}