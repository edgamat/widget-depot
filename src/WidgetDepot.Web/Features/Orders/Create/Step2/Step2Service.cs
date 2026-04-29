using System.Net;
using System.Net.Http.Json;

namespace WidgetDepot.Web.Features.Orders.Create.Step2;

public class Step2Service
{
    private readonly HttpClient _httpClient;

    public Step2Service(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}