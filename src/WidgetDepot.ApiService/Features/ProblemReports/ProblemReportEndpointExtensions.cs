using WidgetDepot.ApiService.Features.ProblemReports.GetOrderForProblemReport;

namespace WidgetDepot.ApiService.Features.ProblemReports;

public static class ProblemReportEndpointExtensions
{
    public static IEndpointRouteBuilder MapProblemReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetOrderForProblemReport();

        return app;
    }
}