using WidgetDepot.ApiService.Features.Accounts;
using WidgetDepot.ApiService.Features.Widgets;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapWidgetEndpoints();
        app.MapAccountEndpoints();

        return app;
    }
}