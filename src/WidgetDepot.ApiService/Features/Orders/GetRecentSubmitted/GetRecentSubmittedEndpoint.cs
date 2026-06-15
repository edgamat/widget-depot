using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.GetRecentSubmitted;

public static class GetRecentSubmittedEndpoint
{
    public static IEndpointRouteBuilder MapGetRecentSubmitted(this IEndpointRouteBuilder app)
    {
        app.MapGet(OrderEndpoints.GetRecentSubmitted, async (
            ClaimsPrincipal user,
            IRequestHandler<GetRecentSubmittedQuery, IReadOnlyList<GetRecentSubmittedOrderResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new GetRecentSubmittedQuery(customerId), cancellationToken);

            return Results.Ok(result);
        })
        .WithName("GetRecentSubmitted")
        .RequireAuthorization();

        return app;
    }
}