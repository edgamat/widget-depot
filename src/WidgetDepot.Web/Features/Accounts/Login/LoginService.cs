using System.Net;
using System.Text.Json;

namespace WidgetDepot.Web.Features.Accounts.Login;

public class LoginService(HttpClient httpClient)
{
    public async Task<LoginResult> LoginAsync(LoginFormModel form, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            form.Email,
            form.Password
        };

        var response = await httpClient.PostAsJsonAsync("/accounts/login", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var customerId = root.GetProperty("customerId").GetInt32();
            var email = root.GetProperty("email").GetString() ?? string.Empty;
            var firstName = root.GetProperty("firstName").GetString() ?? string.Empty;

            return new LoginResult.Success(customerId, email, firstName);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("errorCode", out var errorCode) &&
                errorCode.GetString() == "InvalidCredentials")
            {
                return new LoginResult.InvalidCredentials();
            }
        }

        return new LoginResult.Failure();
    }
}