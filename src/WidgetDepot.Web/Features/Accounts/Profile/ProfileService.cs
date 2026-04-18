using System.Net;
using System.Text.Json;

namespace WidgetDepot.Web.Features.Accounts.Profile;

public class ProfileService(HttpClient httpClient)
{
    public async Task<ProfileGetResult> GetProfileAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "/accounts/profile");
        message.Headers.Add("X-Customer-Id", customerId.ToString());

        var response = await httpClient.SendAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(cancellationToken);
            return new ProfileGetResult.Success(profile!);
        }

        return new ProfileGetResult.Failure();
    }

    public async Task<ProfileUpdateResult> UpdateProfileAsync(int customerId, ProfileFormModel form, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, "/accounts/profile")
        {
            Content = JsonContent.Create(new { form.FirstName, form.LastName, form.Email })
        };
        message.Headers.Add("X-Customer-Id", customerId.ToString());

        var response = await httpClient.SendAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(cancellationToken);
            return new ProfileUpdateResult.Success(profile!);
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("errorCode", out var errorCode) &&
                errorCode.GetString() == "EmailAlreadyRegistered")
            {
                return new ProfileUpdateResult.DuplicateEmail();
            }
        }

        return new ProfileUpdateResult.Failure();
    }
}