using System.Net;
using System.Net.Http.Json;

namespace WidgetDepot.Web.Features.Orders.Create.Step2;

public abstract record GetDraftOrderResult
{
    public record Success(GetDraftOrderResponse Order) : GetDraftOrderResult;
    public record NotFound : GetDraftOrderResult;
    public record Forbidden : GetDraftOrderResult;
    public record Failure : GetDraftOrderResult;
}

public abstract record GetProfileAddressesResult
{
    public record Success(ProfileAddressesResponse Profile) : GetProfileAddressesResult;
    public record Failure : GetProfileAddressesResult;
}

public class Step2Service
{
    private readonly HttpClient _httpClient;

    public Step2Service(HttpClient httpClient)
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

    public async Task<SaveAddressesResult> SaveAddressesAsync(int orderId, Step2FormModel form, CancellationToken cancellationToken = default)
    {
        var request = new SaveAddressesRequest(
            new AddressRequest(
                form.ShippingRecipientName,
                form.ShippingStreetLine1,
                NullIfEmpty(form.ShippingStreetLine2),
                form.ShippingCity,
                form.ShippingState,
                form.ShippingZipCode),
            new AddressRequest(
                form.BillingRecipientName,
                form.BillingStreetLine1,
                NullIfEmpty(form.BillingStreetLine2),
                form.BillingCity,
                form.BillingState,
                form.BillingZipCode));

        var response = await _httpClient.PostAsJsonAsync($"/orders/{orderId}/addresses", request, cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.NoContent => new SaveAddressesResult.Success(),
            HttpStatusCode.NotFound => new SaveAddressesResult.NotFound(),
            HttpStatusCode.Forbidden => new SaveAddressesResult.Forbidden(),
            _ => new SaveAddressesResult.Failure()
        };
    }

    public async Task<GetProfileAddressesResult> GetProfileAddressesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/accounts/profile", cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var profile = await response.Content.ReadFromJsonAsync<ProfileAddressesResponse>(cancellationToken);
            return profile is null
                ? new GetProfileAddressesResult.Failure()
                : new GetProfileAddressesResult.Success(profile);
        }

        return new GetProfileAddressesResult.Failure();
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}