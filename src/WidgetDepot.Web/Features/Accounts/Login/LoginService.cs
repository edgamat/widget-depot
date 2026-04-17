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
            var customer = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
            return new LoginResult.Success(customer!);
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