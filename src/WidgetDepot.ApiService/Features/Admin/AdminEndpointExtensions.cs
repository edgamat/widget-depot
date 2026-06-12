using WidgetDepot.ApiService.Features.Admin.Customers;

namespace WidgetDepot.ApiService.Features.Admin;

public static class AdminEndpointExtensions
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCustomerAdminEndpoints();

        return app;
    }
}