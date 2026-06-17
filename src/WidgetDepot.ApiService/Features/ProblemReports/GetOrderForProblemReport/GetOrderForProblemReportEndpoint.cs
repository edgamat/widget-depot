using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.ProblemReports.GetOrderForProblemReport;

public static class GetOrderForProblemReportEndpoint
{
    public static IEndpointRouteBuilder MapGetOrderForProblemReport(this IEndpointRouteBuilder app)
    {
        app.MapGet("/problem-reports/order-lookup/{orderNumber:int}", async (
            int orderNumber,
            ClaimsPrincipal user,
            IRequestHandler<GetOrderForProblemReportQuery, object> handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new GetOrderForProblemReportQuery(orderNumber, customerId), cancellationToken);

            return result switch
            {
                GetOrderForProblemReportNotFound => Results.NotFound(),
                GetOrderForProblemReportNotSubmitted => Results.UnprocessableEntity(),
                GetOrderForProblemReportResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("GetOrderForProblemReport")
        .RequireAuthorization();

        return app;
    }
}