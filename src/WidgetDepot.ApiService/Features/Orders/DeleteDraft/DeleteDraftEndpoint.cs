using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.DeleteDraft;

public static class DeleteDraftEndpoint
{
    public static IEndpointRouteBuilder MapDeleteDraft(this IEndpointRouteBuilder app)
    {
        app.MapDelete(OrderEndpoints.DeleteDraft, async (
            int orderId,
            ClaimsPrincipal user,
            IRequestHandler<DeleteDraftCommand, DeleteDraftError?> handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var error = await handler.HandleAsync(new DeleteDraftCommand(orderId, customerId), cancellationToken);

            return error switch
            {
                DeleteDraftError.OrderNotFound => Results.NotFound(),
                DeleteDraftError.Forbidden => Results.Forbid(),
                null => Results.NoContent(),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("DeleteDraft")
        .RequireAuthorization();

        return app;
    }
}