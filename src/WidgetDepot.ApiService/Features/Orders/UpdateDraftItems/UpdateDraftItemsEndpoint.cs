using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Orders.UpdateDraftItems;

public static class UpdateDraftItemsEndpoint
{
    public static IEndpointRouteBuilder MapUpdateDraftItems(this IEndpointRouteBuilder app)
    {
        app.MapPut(OrderEndpoints.UpdateDraftItems, async (
            int orderId,
            UpdateDraftItemsRequest request,
            ClaimsPrincipal user,
            UpdateDraftItemsHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(orderId, customerId, request, cancellationToken);

            return result switch
            {
                UpdateDraftItemsError.OrderNotFound => Results.NotFound(),
                UpdateDraftItemsError.Forbidden => Results.Forbid(),
                UpdateDraftItemsError.NotDraft => Results.Conflict(),
                UpdateDraftItemsError.WidgetNotFound error => Results.BadRequest(new { error.WidgetId }),
                null => Results.NoContent(),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("UpdateDraftItems")
        .RequireAuthorization();

        return app;
    }
}