using WidgetDepot.ApiService.Features.Widgets.Search;

namespace WidgetDepot.ApiService.Features.Widgets;

public static class WidgetEndpointExtensions
{
    public static IEndpointRouteBuilder MapWidgetEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapSearchWidget();
        
        return app;
    }
}
