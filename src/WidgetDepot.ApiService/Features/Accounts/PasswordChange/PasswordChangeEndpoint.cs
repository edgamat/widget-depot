using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Accounts.PasswordChange;

public static class PasswordChangeEndpoint
{
    public static IEndpointRouteBuilder MapPasswordChange(this IEndpointRouteBuilder app)
    {
        app.MapPut("/accounts/password", async (ChangePasswordRequest request, ClaimsPrincipal user, IRequestHandler<ChangePasswordCommand, object> handler, CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new ChangePasswordCommand(customerId, request), cancellationToken);

            return result switch
            {
                PasswordChangeError.NotFound => Results.NotFound(),
                PasswordChangeError.IncorrectPassword => Results.Problem(
                    detail: "The current password is incorrect.",
                    statusCode: 409,
                    extensions: new Dictionary<string, object?> { ["errorCode"] = "IncorrectPassword" }),
                ChangePasswordSuccess => Results.NoContent(),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("ChangePassword")
        .RequireAuthorization();

        return app;
    }
}