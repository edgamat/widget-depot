using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Orders.GetDrafts;

public static class GetDraftsEndpoint
{
    public static IEndpointRouteBuilder MapGetDrafts(this IEndpointRouteBuilder app)
    {
        app.MapGet(OrderEndpoints.GetDrafts, async (
            ClaimsPrincipal user,
            GetDraftsHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(customerId, cancellationToken);

            return Results.Ok(result);
        })
        .WithName("GetDrafts")
        .RequireAuthorization();

        return app;
    }
}