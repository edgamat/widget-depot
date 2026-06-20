using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.ProblemReports.SubmitProblemReport;

internal record SubmitProblemReportBody(
    int OrderId,
    IReadOnlyList<SubmitProblemReportItemRequest> Items,
    string? Notes);

public static class SubmitProblemReportEndpoint
{
    public static IEndpointRouteBuilder MapSubmitProblemReport(this IEndpointRouteBuilder app)
    {
        app.MapPost("/problem-reports", async (
            SubmitProblemReportBody body,
            ClaimsPrincipal user,
            IRequestHandler<SubmitProblemReportRequest, object> handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var request = new SubmitProblemReportRequest(body.OrderId, customerId, body.Items, body.Notes);
            var result = await handler.HandleAsync(request, cancellationToken);

            return result switch
            {
                SubmitProblemReportOrderNotFound => Results.NotFound(),
                SubmitProblemReportOrderNotSubmitted => Results.UnprocessableEntity(),
                SubmitProblemReportInvalidItems => Results.BadRequest(),
                SubmitProblemReportResponse response => Results.Created($"/problem-reports/{response.ProblemReportId}", response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("SubmitProblemReport")
        .RequireAuthorization();

        return app;
    }
}