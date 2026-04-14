using System.Net;
using System.Text.Json;

namespace WidgetDepot.Web.Features.Accounts.Register;

public class RegisterService(HttpClient httpClient)
{
    public async Task<RegisterResult> RegisterAsync(RegisterFormModel form, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            form.FirstName,
            form.LastName,
            form.Email,
            form.Password
        };

        var response = await httpClient.PostAsJsonAsync("/accounts/register", request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return new RegisterResult.Success();

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("errorCode", out var errorCode) &&
                errorCode.GetString() == "EmailAlreadyRegistered")
            {
                return new RegisterResult.EmailAlreadyRegistered();
            }
        }

        return new RegisterResult.Failure();
    }
}