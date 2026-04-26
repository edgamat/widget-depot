using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Accounts.Profile;

public static class ProfileEndpoint
{
    public static IEndpointRouteBuilder MapProfile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/accounts/profile", async (ClaimsPrincipal user, ProfileHandler handler, CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.GetAsync(customerId, cancellationToken);

            return result switch
            {
                ProfileError.NotFound => Results.NotFound(),
                GetProfileResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("GetProfile")
        .RequireAuthorization();

        app.MapPut("/accounts/profile", async (UpdateProfileRequest request, ClaimsPrincipal user, ProfileHandler handler, CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.UpdateAsync(customerId, request, cancellationToken);

            return result switch
            {
                ProfileError.NotFound => Results.NotFound(),
                ProfileError.EmailAlreadyRegistered => Results.Problem(
                    detail: "The email address is already registered.",
                    statusCode: 409,
                    extensions: new Dictionary<string, object?> { ["errorCode"] = "EmailAlreadyRegistered" }),
                UpdateProfileResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("UpdateProfile")
        .RequireAuthorization();

        return app;
    }
}