using System.Security.Claims;

using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.ProblemReports.GetMyProblemReports;

public static class GetMyProblemReportsEndpoint
{
    public static IEndpointRouteBuilder MapGetMyProblemReports(this IEndpointRouteBuilder app)
    {
        app.MapGet("/problem-reports", async (
            ClaimsPrincipal user,
            IRequestHandler<GetMyProblemReportsQuery, IReadOnlyList<GetMyProblemReportsResponseItem>> handler,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new GetMyProblemReportsQuery(customerId), cancellationToken);

            return Results.Ok(result);
        })
        .WithName("GetMyProblemReports")
        .RequireAuthorization();

        return app;
    }
}