using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.Logging;

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
    private readonly ILogger<Step2Service> _logger;

    public Step2Service(HttpClient httpClient, ILogger<Step2Service> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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

    public async Task<SaveProfileAddressesResult> SaveProfileAddressesAsync(
        Step2FormModel form,
        bool saveShipping,
        bool saveBilling,
        CancellationToken cancellationToken = default)
    {
        var getResponse = await _httpClient.GetAsync("/accounts/profile", cancellationToken);
        if (!getResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get profile for address update. StatusCode: {StatusCode}", getResponse.StatusCode);
            return new SaveProfileAddressesResult.Failure();
        }

        var profile = await getResponse.Content.ReadFromJsonAsync<FullProfileData>(cancellationToken);
        if (profile is null)
        {
            _logger.LogWarning("Failed to deserialize profile response for address update.");
            return new SaveProfileAddressesResult.Failure();
        }

        var shippingAddress = saveShipping ? ToProfileAddress(form, shipping: true) : profile.ShippingAddress;
        var billingAddress = saveBilling ? ToProfileAddress(form, shipping: false) : profile.BillingAddress;

        var request = new FullProfileData(profile.FirstName, profile.LastName, profile.Email, shippingAddress, billingAddress);
        var putResponse = await _httpClient.PutAsJsonAsync("/accounts/profile", request, cancellationToken);

        if (!putResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to update profile addresses. StatusCode: {StatusCode}", putResponse.StatusCode);
            return new SaveProfileAddressesResult.Failure();
        }

        return new SaveProfileAddressesResult.Success();
    }

    private static ProfileAddressData ToProfileAddress(Step2FormModel form, bool shipping) =>
        shipping
            ? new ProfileAddressData(form.ShippingRecipientName, form.ShippingStreetLine1, NullIfEmpty(form.ShippingStreetLine2), form.ShippingCity, form.ShippingState, form.ShippingZipCode)
            : new ProfileAddressData(form.BillingRecipientName, form.BillingStreetLine1, NullIfEmpty(form.BillingStreetLine2), form.BillingCity, form.BillingState, form.BillingZipCode);

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}