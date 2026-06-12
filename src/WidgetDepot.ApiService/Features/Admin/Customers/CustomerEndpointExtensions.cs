namespace WidgetDepot.ApiService.Features.Admin.Customers;

public static class CustomerEndpointExtensions
{
    public static IEndpointRouteBuilder MapCustomerAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/customers", async (int page, int pageSize, GetCustomerListHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.GetAsync(page, pageSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAdminCustomerList")
        .RequireAuthorization("IsAdmin");

        app.MapGet("/admin/customers/{id:int}", async (int id, GetCustomerProfileHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.GetAsync(id, cancellationToken);

            return result switch
            {
                CustomerProfileError.NotFound => Results.NotFound(),
                CustomerProfileResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("GetAdminCustomerProfile")
        .RequireAuthorization("IsAdmin");

        return app;
    }
}