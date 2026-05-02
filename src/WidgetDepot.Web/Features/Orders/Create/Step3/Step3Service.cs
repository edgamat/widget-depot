using System.Net;
using System.Net.Http.Json;

namespace WidgetDepot.Web.Features.Orders.Create.Step3;

public class Step3Service
{
    private readonly HttpClient _httpClient;

    public Step3Service(HttpClient httpClient)
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

    public async Task<CalculateShippingResult> CalculateShippingAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"/orders/{orderId}/shipping-estimate", null, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<CalculateShippingResponse>(cancellationToken);
            return result is null
                ? new CalculateShippingResult.Failure()
                : new CalculateShippingResult.Success(result.EstimatedCost, result.Currency);
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => new CalculateShippingResult.NotFound(),
            HttpStatusCode.Forbidden => new CalculateShippingResult.Forbidden(),
            HttpStatusCode.UnprocessableEntity => new CalculateShippingResult.NoShippingAddress(),
            HttpStatusCode.BadGateway => new CalculateShippingResult.ApiFailure(),
            _ => new CalculateShippingResult.Failure()
        };
    }
}