using WidgetDepot.ApiService.Features.Orders.CalculateShipping;
using WidgetDepot.ApiService.Features.Orders.CreateDraft;
using WidgetDepot.ApiService.Features.Orders.GetDraftOrder;
using WidgetDepot.ApiService.Features.Orders.SaveAddresses;

namespace WidgetDepot.ApiService.Features.Orders;

public static class OrderEndpointExtensions
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateDraftOrder();
        app.MapGetDraftOrder();
        app.MapSaveAddresses();
        app.MapCalculateShipping();

        return app;
    }
}