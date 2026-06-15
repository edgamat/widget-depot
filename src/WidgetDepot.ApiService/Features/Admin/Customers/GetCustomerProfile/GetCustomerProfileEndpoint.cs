using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerProfile;

public static class GetCustomerProfileEndpoint
{
    public static IEndpointRouteBuilder MapGetCustomerProfile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/customers/{id:int}", async (int id, IRequestHandler<GetCustomerProfileQuery, object> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetCustomerProfileQuery(id), cancellationToken);

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