using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Orders.RetransmitOrder;

public static class RetransmitOrderEndpoint
{
    public static IEndpointRouteBuilder MapRetransmitOrder(this IEndpointRouteBuilder app)
    {
        app.MapPost(OrderEndpoints.RetransmitOrder, async (
            int orderId,
            ClaimsPrincipal user,
            RetransmitOrderHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(orderId, customerId, cancellationToken);

            return result switch
            {
                RetransmitOrderNotFound => Results.NotFound(),
                RetransmitOrderInvalidStatus => Results.Conflict(),
                RetransmitOrderResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("RetransmitOrder")
        .RequireAuthorization();

        return app;
    }
}