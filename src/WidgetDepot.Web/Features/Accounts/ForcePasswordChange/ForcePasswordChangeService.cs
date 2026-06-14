namespace WidgetDepot.Web.Features.Accounts.ForcePasswordChange;

public class ForcePasswordChangeService(HttpClient httpClient)
{
    public async Task<ForcePasswordChangeResult> ChangeAsync(ForcePasswordChangeFormModel form, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            form.NewPassword
        };

        var response = await httpClient.PutAsJsonAsync("/accounts/force-password-change", request, cancellationToken);

        return response.IsSuccessStatusCode
            ? new ForcePasswordChangeResult.Success()
            : new ForcePasswordChangeResult.Failure();
    }
}