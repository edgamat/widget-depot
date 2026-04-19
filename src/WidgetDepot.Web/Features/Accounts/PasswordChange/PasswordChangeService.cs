using System.Net;
using System.Text.Json;

namespace WidgetDepot.Web.Features.Accounts.PasswordChange;

public class PasswordChangeService(HttpClient httpClient)
{
    public async Task<ChangePasswordResult> ChangeAsync(PasswordChangeFormModel form, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            form.CurrentPassword,
            form.NewPassword
        };

        var response = await httpClient.PutAsJsonAsync("/accounts/password", request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return new ChangePasswordResult.Success();

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("errorCode", out var errorCode) &&
                errorCode.GetString() == "IncorrectPassword")
            {
                return new ChangePasswordResult.IncorrectPassword();
            }
        }

        return new ChangePasswordResult.Failure();
    }
}