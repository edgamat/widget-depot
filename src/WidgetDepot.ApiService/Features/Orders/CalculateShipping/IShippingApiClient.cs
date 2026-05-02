namespace WidgetDepot.ApiService.Features.Orders.CalculateShipping;

public record ShippingEstimateRequest(
    string OriginPostalCode,
    string OriginCountry,
    string DestinationPostalCode,
    string DestinationCountry,
    decimal WeightLbs);

public abstract record ShippingEstimateResult
{
    public record Success(decimal EstimatedCost, string Currency) : ShippingEstimateResult;
    public record Failure(string Reason) : ShippingEstimateResult;
}

public interface IShippingApiClient
{
    Task<ShippingEstimateResult> GetEstimateAsync(ShippingEstimateRequest request, CancellationToken cancellationToken);
}