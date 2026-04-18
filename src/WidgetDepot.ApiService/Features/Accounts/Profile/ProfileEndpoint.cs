using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Accounts.Profile;

public static class ProfileEndpoint
{
    public static IEndpointRouteBuilder MapProfile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/accounts/profile", async (ClaimsPrincipal user, HttpContext httpContext, ProfileHandler handler, CancellationToken cancellationToken) =>
        {
            if (!TryGetCustomerId(user, httpContext, out var customerId))
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

        app.MapPut("/accounts/profile", async (UpdateProfileRequest request, ClaimsPrincipal user, HttpContext httpContext, ProfileHandler handler, CancellationToken cancellationToken) =>
        {
            if (!TryGetCustomerId(user, httpContext, out var customerId))
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

    private static bool TryGetCustomerId(ClaimsPrincipal user, HttpContext httpContext, out int customerId)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(claim, out customerId))
            return true;

        // Service-to-service fallback: Web app forwards the authenticated customer's ID via header.
        // The ApiService is not externally exposed in Aspire, so this header is trusted.
        var header = httpContext.Request.Headers["X-Customer-Id"].FirstOrDefault();
        return int.TryParse(header, out customerId);
    }
}