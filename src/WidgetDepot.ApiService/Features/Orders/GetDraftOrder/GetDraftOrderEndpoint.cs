using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Orders.GetDraftOrder;

public static class GetDraftOrderEndpoint
{
    public static IEndpointRouteBuilder MapGetDraftOrder(this IEndpointRouteBuilder app)
    {
        app.MapGet(OrderEndpoints.GetDraftOrder, async (
            int orderId,
            ClaimsPrincipal user,
            GetDraftOrderHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(orderId, customerId, cancellationToken);

            return result switch
            {
                GetDraftOrderError.OrderNotFound => Results.NotFound(),
                GetDraftOrderError.Forbidden => Results.Forbid(),
                GetDraftOrderResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("GetDraftOrder")
        .RequireAuthorization();

        return app;
    }
}
