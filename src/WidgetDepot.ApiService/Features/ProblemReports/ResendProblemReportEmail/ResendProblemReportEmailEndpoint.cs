using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.ProblemReports.ResendProblemReportEmail;

public static class ResendProblemReportEmailEndpoint
{
    public static IEndpointRouteBuilder MapResendProblemReportEmail(this IEndpointRouteBuilder app)
    {
        app.MapPost("/problem-reports/{id:int}/resend", async (
            int id,
            ClaimsPrincipal user,
            IRequestHandler<ResendProblemReportEmailCommand, object> handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var command = new ResendProblemReportEmailCommand(id, customerId);
            var result = await handler.HandleAsync(command, cancellationToken);

            return result switch
            {
                ResendProblemReportEmailNotFound => Results.NotFound(),
                ResendProblemReportEmailFailed => Results.Problem(statusCode: 500),
                ResendProblemReportEmailSuccess => Results.Ok(),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("ResendProblemReportEmail")
        .RequireAuthorization();

        return app;
    }
}