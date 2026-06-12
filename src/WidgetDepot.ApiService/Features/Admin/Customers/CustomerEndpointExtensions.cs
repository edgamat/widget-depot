using WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerList;
using WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerProfile;

namespace WidgetDepot.ApiService.Features.Admin.Customers;

public static class CustomerEndpointExtensions
{
    public static IEndpointRouteBuilder MapCustomerAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetCustomerList();
        app.MapGetCustomerProfile();

        return app;
    }
}