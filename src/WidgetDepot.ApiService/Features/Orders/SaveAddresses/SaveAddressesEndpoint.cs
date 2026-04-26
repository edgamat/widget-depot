using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Orders.SaveAddresses;

public static class SaveAddressesEndpoint
{
    public static IEndpointRouteBuilder MapSaveAddresses(this IEndpointRouteBuilder app)
    {
        app.MapPost(OrderEndpoints.SaveAddresses, async (
            int orderId,
            SaveAddressesRequest request,
            ClaimsPrincipal user,
            SaveAddressesHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(orderId, customerId, request, cancellationToken);

            return result switch
            {
                SaveAddressesError.OrderNotFound => Results.NotFound(),
                SaveAddressesError.Forbidden => Results.Forbid(),
                SaveAddressesError.InvalidRequest error => Results.ValidationProblem(error.Errors),
                null => Results.NoContent(),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("SaveAddresses")
        .RequireAuthorization();

        return app;
    }
}