using WidgetDepot.ApiService.Features.Accounts;
using WidgetDepot.ApiService.Features.Admin;
using WidgetDepot.ApiService.Features.Orders;
using WidgetDepot.ApiService.Features.ProblemReports;
using WidgetDepot.ApiService.Features.Widgets;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapWidgetEndpoints();
        app.MapAccountEndpoints();
        app.MapOrderEndpoints();
        app.MapAdminEndpoints();
        app.MapProblemReportEndpoints();

        return app;
    }
}