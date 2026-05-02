namespace WidgetDepot.ApiService.Features.Orders.CalculateShipping;

public class AcmeShippingApiClient : IShippingApiClient
{
    private readonly HttpClient _httpClient;

    public AcmeShippingApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ShippingEstimateResult> GetEstimateAsync(ShippingEstimateRequest request, CancellationToken cancellationToken)
    {
        var body = new
        {
            origin = new { postalCode = request.OriginPostalCode, country = request.OriginCountry },
            destination = new { postalCode = request.DestinationPostalCode, country = request.DestinationCountry },
            package = new { weightLbs = (double)request.WeightLbs }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("estimates", body, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EstimateResponse>(cancellationToken);
                return new ShippingEstimateResult.Success(result!.EstimatedCost, result.Currency);
            }

            return new ShippingEstimateResult.Failure($"Received HTTP {(int)response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return new ShippingEstimateResult.Failure(ex.Message);
        }
    }

    private record EstimateResponse(decimal EstimatedCost, string Currency);
}