using WidgetDepot.ApiService.Features.Admin.Customers.DemoteCustomer;
using WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerList;
using WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerProfile;
using WidgetDepot.ApiService.Features.Admin.Customers.PromoteCustomer;
using WidgetDepot.ApiService.Features.Admin.Customers.ResetCustomerPassword;
using WidgetDepot.ApiService.Features.Admin.Customers.UpdateCustomerEmail;

namespace WidgetDepot.ApiService.Features.Admin.Customers;

public static class CustomerEndpointExtensions
{
    public static IEndpointRouteBuilder MapCustomerAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetCustomerList();
        app.MapGetCustomerProfile();
        app.MapUpdateCustomerEmail();
        app.MapResetCustomerPassword();
        app.MapPromoteCustomer();
        app.MapDemoteCustomer();

        return app;
    }
}