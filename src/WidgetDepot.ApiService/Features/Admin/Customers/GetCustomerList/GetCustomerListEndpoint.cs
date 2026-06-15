using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerList;

public static class GetCustomerListEndpoint
{
    public static IEndpointRouteBuilder MapGetCustomerList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/customers", async (int page, int pageSize, IRequestHandler<GetCustomerListQuery, GetCustomerListResponse> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetCustomerListQuery(page, pageSize), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAdminCustomerList")
        .RequireAuthorization("IsAdmin");

        return app;
    }
}