using WidgetDepot.ApiService.Features.ProblemReports.GetOrderForProblemReport;
using WidgetDepot.ApiService.Features.ProblemReports.SubmitProblemReport;

namespace WidgetDepot.ApiService.Features.ProblemReports;

public static class ProblemReportEndpointExtensions
{
    public static IEndpointRouteBuilder MapProblemReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetOrderForProblemReport();
        app.MapSubmitProblemReport();

        return app;
    }
}