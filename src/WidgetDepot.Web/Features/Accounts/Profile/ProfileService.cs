using System.Net;
using System.Text.Json;

namespace WidgetDepot.Web.Features.Accounts.Profile;

public class ProfileService(HttpClient httpClient)
{
    public async Task<LoadProfileResult> LoadAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("/accounts/profile", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(cancellationToken);
            return new LoadProfileResult.Success(profile!);
        }

        return new LoadProfileResult.Failure();
    }

    public async Task<UpdateProfileResult> UpdateAsync(ProfileFormModel form, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            form.FirstName,
            form.LastName,
            form.Email,
            ShippingAddress = form.ShippingAddress.HasAnyValue()
                ? new
                {
                    form.ShippingAddress.RecipientName,
                    form.ShippingAddress.StreetLine1,
                    form.ShippingAddress.StreetLine2,
                    form.ShippingAddress.City,
                    form.ShippingAddress.State,
                    form.ShippingAddress.ZipCode
                }
                : (object?)null,
            BillingAddress = form.BillingAddress.HasAnyValue()
                ? new
                {
                    form.BillingAddress.RecipientName,
                    form.BillingAddress.StreetLine1,
                    form.BillingAddress.StreetLine2,
                    form.BillingAddress.City,
                    form.BillingAddress.State,
                    form.BillingAddress.ZipCode
                }
                : (object?)null
        };

        var response = await httpClient.PutAsJsonAsync("/accounts/profile", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(cancellationToken);
            return new UpdateProfileResult.Success(profile!);
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("errorCode", out var errorCode) &&
                errorCode.GetString() == "EmailAlreadyRegistered")
            {
                return new UpdateProfileResult.DuplicateEmail();
            }
        }

        return new UpdateProfileResult.Failure();
    }
}