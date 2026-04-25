using WidgetDepot.ApiService.Features.Orders.CreateDraft;

namespace WidgetDepot.ApiService.Features.Orders;

public static class OrderEndpointExtensions
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateDraftOrder();

        return app;
    }
}