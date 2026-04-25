using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Orders.CreateDraft;

public static class CreateDraftOrderEndpoint
{
    public static IEndpointRouteBuilder MapCreateDraftOrder(this IEndpointRouteBuilder app)
    {
        app.MapPost(OrderEndpoints.CreateDraft, async (
            CreateDraftOrderRequest request,
            ClaimsPrincipal user,
            CreateDraftOrderHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetCustomerId(user, out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(customerId, request, cancellationToken);

            return result switch
            {
                CreateDraftOrderError.WidgetNotFound => Results.NotFound(),
                CreateDraftOrderResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("CreateDraftOrder")
        .RequireAuthorization();

        return app;
    }

    private static bool TryGetCustomerId(ClaimsPrincipal user, out int customerId)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out customerId);
    }
}