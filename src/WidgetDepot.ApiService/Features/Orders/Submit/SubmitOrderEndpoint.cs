using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Orders.Submit;

public static class SubmitOrderEndpoint
{
    public static IEndpointRouteBuilder MapSubmitOrder(this IEndpointRouteBuilder app)
    {
        app.MapPost(OrderEndpoints.SubmitOrder, async (
            int orderId,
            ClaimsPrincipal user,
            SubmitOrderHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(orderId, customerId, cancellationToken);

            return result switch
            {
                SubmitOrderError.OrderNotFound => Results.NotFound(),
                SubmitOrderError.Forbidden => Results.Forbid(),
                SubmitOrderError.InvalidOrderState => Results.Conflict(),
                SubmitOrderError.IncompleteOrder error => Results.Problem(detail: error.Reason, statusCode: 422),
                SubmitOrderResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("SubmitOrder")
        .RequireAuthorization();

        return app;
    }
}