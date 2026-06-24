using System.Security.Claims;

using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.ProblemReports.CreateTestProblemReport;

internal record CreateTestProblemReportBody(int OrderId);

public static class CreateTestProblemReportEndpoint
{
    public static IEndpointRouteBuilder MapCreateTestProblemReport(this IEndpointRouteBuilder app)
    {
        app.MapPost("/test/problem-reports", async (
            CreateTestProblemReportBody body,
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var customerId))
                return Results.Unauthorized();

            var order = await db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == body.OrderId && o.CustomerId == customerId, cancellationToken);

            if (order is null)
                return Results.NotFound();

            var firstItem = order.Items.FirstOrDefault();
            if (firstItem is null)
                return Results.UnprocessableEntity();

            var report = new ProblemReport
            {
                OrderId = body.OrderId,
                CreatedAt = DateTime.UtcNow,
                EmailSent = false,
                Items = [new ProblemReportItem { OrderItemId = firstItem.Id, IssueType = IssueType.Damaged }]
            };

            db.ProblemReports.Add(report);
            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { problemReportId = report.Id });
        })
        .RequireAuthorization();

        return app;
    }
}
