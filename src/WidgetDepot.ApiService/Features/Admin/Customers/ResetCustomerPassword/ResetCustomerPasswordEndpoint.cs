using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.ResetCustomerPassword;

public static class ResetCustomerPasswordEndpoint
{
    public static IEndpointRouteBuilder MapResetCustomerPassword(this IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/customers/{id:int}/reset-password", async (int id, IRequestHandler<ResetCustomerPasswordCommand, object> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new ResetCustomerPasswordCommand(id), cancellationToken);

            return result switch
            {
                ResetCustomerPasswordError.NotFound => Results.NotFound(),
                ResetCustomerPasswordSuccess success => Results.Ok(success),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("ResetAdminCustomerPassword")
        .RequireAuthorization("IsAdmin");

        return app;
    }
}