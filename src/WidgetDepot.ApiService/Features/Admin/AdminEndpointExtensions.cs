using WidgetDepot.ApiService.Features.Admin.Customers;
using WidgetDepot.ApiService.Features.Admin.Orders;

namespace WidgetDepot.ApiService.Features.Admin;

public static class AdminEndpointExtensions
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCustomerAdminEndpoints();
        app.MapOrderAdminEndpoints();

        return app;
    }
}