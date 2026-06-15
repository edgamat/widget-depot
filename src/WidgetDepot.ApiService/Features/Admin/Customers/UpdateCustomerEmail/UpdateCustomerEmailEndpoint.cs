using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.UpdateCustomerEmail;

public static class UpdateCustomerEmailEndpoint
{
    public static IEndpointRouteBuilder MapUpdateCustomerEmail(this IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/customers/{id:int}/email", async (int id, UpdateCustomerEmailRequest request, IRequestHandler<UpdateCustomerEmailCommand, object> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new UpdateCustomerEmailCommand(id, request), cancellationToken);

            return result switch
            {
                UpdateCustomerEmailError.NotFound => Results.NotFound(),
                UpdateCustomerEmailError.EmailAlreadyInUse => Results.Conflict(),
                UpdateCustomerEmailSuccess => Results.Ok(),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("UpdateAdminCustomerEmail")
        .RequireAuthorization("IsAdmin");

        return app;
    }
}