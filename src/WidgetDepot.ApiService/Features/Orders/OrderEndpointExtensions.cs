using WidgetDepot.ApiService.Features.Orders.CreateDraft;
using WidgetDepot.ApiService.Features.Orders.SaveAddresses;

namespace WidgetDepot.ApiService.Features.Orders;

public static class OrderEndpointExtensions
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateDraftOrder();
        app.MapSaveAddresses();

        return app;
    }
}