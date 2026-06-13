using WidgetDepot.ApiService.Features.Admin.Orders.GetAdminOrderByNumber;

namespace WidgetDepot.ApiService.Features.Admin.Orders;

public static class OrderAdminEndpointExtensions
{
    public static IEndpointRouteBuilder MapOrderAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetAdminOrderByNumber();
        return app;
    }
}