using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Orders.CalculateShipping;

public static class CalculateShippingEndpoint
{
    public static IEndpointRouteBuilder MapCalculateShipping(this IEndpointRouteBuilder app)
    {
        app.MapPost(OrderEndpoints.CalculateShipping, async (
            int orderId,
            ClaimsPrincipal user,
            CalculateShippingHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(orderId, customerId, cancellationToken);

            return result switch
            {
                CalculateShippingError.OrderNotFound => Results.NotFound(),
                CalculateShippingError.Forbidden => Results.Forbid(),
                CalculateShippingError.NoShippingAddress => Results.Problem(
                    detail: "Shipping address is required before calculating shipping.",
                    statusCode: StatusCodes.Status422UnprocessableEntity),
                CalculateShippingError.ShippingApiFailure error => Results.Problem(
                    detail: error.Reason,
                    statusCode: StatusCodes.Status502BadGateway),
                CalculateShippingResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("CalculateShipping")
        .RequireAuthorization();

        return app;
    }
}