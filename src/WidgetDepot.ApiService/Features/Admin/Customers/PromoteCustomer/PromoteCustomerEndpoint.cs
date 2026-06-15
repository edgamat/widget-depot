using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.PromoteCustomer;

public static class PromoteCustomerEndpoint
{
    public static IEndpointRouteBuilder MapPromoteCustomer(this IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/customers/{id:int}/promote", async (int id, IRequestHandler<PromoteCustomerCommand, object> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new PromoteCustomerCommand(id), cancellationToken);

            return result switch
            {
                PromoteCustomerError.NotFound => Results.NotFound(),
                PromoteCustomerSuccess => Results.Ok(),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("PromoteAdminCustomer")
        .RequireAuthorization("IsAdmin");

        return app;
    }
}