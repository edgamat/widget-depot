using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.GetByOrderNumber;

public static class GetByOrderNumberEndpoint
{
    public static IEndpointRouteBuilder MapGetByOrderNumber(this IEndpointRouteBuilder app)
    {
        app.MapGet(OrderEndpoints.GetByOrderNumber, async (
            int orderNumber,
            ClaimsPrincipal user,
            IRequestHandler<GetByOrderNumberQuery, object> handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new GetByOrderNumberQuery(orderNumber, customerId), cancellationToken);

            return result switch
            {
                GetByOrderNumberNotFound => Results.NotFound(),
                GetByOrderNumberResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("GetByOrderNumber")
        .RequireAuthorization();

        return app;
    }
}