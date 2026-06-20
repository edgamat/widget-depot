using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.ProblemReports.SubmitProblemReport;

public record SubmitProblemReportItemRequest(int OrderItemId, string IssueType);

public record SubmitProblemReportRequest(
    int OrderId,
    int CustomerId,
    IReadOnlyList<SubmitProblemReportItemRequest> Items,
    string? Notes) : IRequest<object>;

public record SubmitProblemReportResponse(int ProblemReportId);

public record SubmitProblemReportOrderNotFound;

public record SubmitProblemReportOrderNotSubmitted;

public record SubmitProblemReportInvalidItems;

public class SubmitProblemReportHandler : IRequestHandler<SubmitProblemReportRequest, object>
{
    private readonly AppDbContext _db;

    public SubmitProblemReportHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<object> HandleAsync(SubmitProblemReportRequest request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CustomerId == request.CustomerId, cancellationToken);

        if (order is null)
            return new SubmitProblemReportOrderNotFound();

        if (order.Status != OrderStatus.Submitted)
            return new SubmitProblemReportOrderNotSubmitted();

        var orderItemIds = order.Items.Select(i => i.Id).ToHashSet();
        var allItemsValid = request.Items.All(i => orderItemIds.Contains(i.OrderItemId));

        if (!allItemsValid)
            return new SubmitProblemReportInvalidItems();

        var report = new ProblemReport
        {
            OrderId = request.OrderId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            Items = [.. request.Items.Select(i => new ProblemReportItem
            {
                OrderItemId = i.OrderItemId,
                IssueType = Enum.Parse<IssueType>(i.IssueType, ignoreCase: true)
            })]
        };

        _db.ProblemReports.Add(report);
        await _db.SaveChangesAsync(cancellationToken);

        return new SubmitProblemReportResponse(report.Id);
    }
}