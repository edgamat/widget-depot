using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.RecreateOrder;

public static class RecreateOrderEndpoint
{
    public static IEndpointRouteBuilder MapRecreateOrder(this IEndpointRouteBuilder app)
    {
        app.MapPost(OrderEndpoints.RecreateOrder, async (
            int orderId,
            ClaimsPrincipal user,
            IRequestHandler<RecreateOrderCommand, object> handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new RecreateOrderCommand(orderId, customerId), cancellationToken);

            return result switch
            {
                RecreateOrderNotFound => Results.NotFound(),
                RecreateOrderInvalidStatus => Results.Conflict(),
                RecreateOrderResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("RecreateOrder")
        .RequireAuthorization();

        return app;
    }
}