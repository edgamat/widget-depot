using WidgetDepot.ApiService.Features.ProblemReports.GetMyProblemReports;
using WidgetDepot.ApiService.Features.ProblemReports.GetOrderForProblemReport;
using WidgetDepot.ApiService.Features.ProblemReports.ResendProblemReportEmail;
using WidgetDepot.ApiService.Features.ProblemReports.SubmitProblemReport;

namespace WidgetDepot.ApiService.Features.ProblemReports;

public static class ProblemReportEndpointExtensions
{
    public static IEndpointRouteBuilder MapProblemReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetMyProblemReports();
        app.MapGetOrderForProblemReport();
        app.MapSubmitProblemReport();
        app.MapResendProblemReportEmail();

        return app;
    }
}