namespace WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerList;

public static class GetCustomerListEndpoint
{
    public static IEndpointRouteBuilder MapGetCustomerList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/customers", async (int page, int pageSize, GetCustomerListHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.GetAsync(page, pageSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAdminCustomerList")
        .RequireAuthorization("IsAdmin");

        return app;
    }
}