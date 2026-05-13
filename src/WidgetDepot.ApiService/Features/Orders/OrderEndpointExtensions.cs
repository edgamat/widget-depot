using WidgetDepot.ApiService.Features.Orders.CalculateShipping;
using WidgetDepot.ApiService.Features.Orders.CreateDraft;
using WidgetDepot.ApiService.Features.Orders.DeleteDraft;
using WidgetDepot.ApiService.Features.Orders.GetByOrderNumber;
using WidgetDepot.ApiService.Features.Orders.GetDraftOrder;
using WidgetDepot.ApiService.Features.Orders.GetDrafts;
using WidgetDepot.ApiService.Features.Orders.GetRecentSubmitted;
using WidgetDepot.ApiService.Features.Orders.SaveAddresses;
using WidgetDepot.ApiService.Features.Orders.Submit;
using WidgetDepot.ApiService.Features.Orders.UpdateDraftItems;

namespace WidgetDepot.ApiService.Features.Orders;

public static class OrderEndpointExtensions
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateDraftOrder();
        app.MapGetDraftOrder();
        app.MapGetDrafts();
        app.MapSaveAddresses();
        app.MapCalculateShipping();
        app.MapDeleteDraft();
        app.MapSubmitOrder();
        app.MapUpdateDraftItems();
        app.MapGetRecentSubmitted();
        app.MapGetByOrderNumber();

        return app;
    }
}