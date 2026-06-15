using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Accounts.ForcePasswordChange;

public static class ForcePasswordChangeEndpoint
{
    public static IEndpointRouteBuilder MapForcePasswordChange(this IEndpointRouteBuilder app)
    {
        app.MapPut("/accounts/force-password-change", async (ForcePasswordChangeRequest request, ClaimsPrincipal user, IRequestHandler<ForcePasswordChangeCommand, object> handler, CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new ForcePasswordChangeCommand(customerId, request), cancellationToken);

            return result switch
            {
                ForcePasswordChangeError.NotFound => Results.NotFound(),
                ForcePasswordChangeSuccess => Results.NoContent(),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("ForcePasswordChange")
        .RequireAuthorization();

        return app;
    }
}