using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.ProblemReports.Email;
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
    private readonly IProblemReportEmailSender _emailSender;

    public SubmitProblemReportHandler(AppDbContext db, IProblemReportEmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender;
    }

    public async Task<object> HandleAsync(SubmitProblemReportRequest request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Widget)
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

        var emailMessage = BuildEmailMessage(request, order);
        var emailSent = await _emailSender.SendAsync(emailMessage, cancellationToken);

        if (emailSent)
        {
            report.EmailSent = true;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new SubmitProblemReportResponse(report.Id);
    }

    private static ProblemReportEmailMessage BuildEmailMessage(SubmitProblemReportRequest request, Order order)
    {
        var itemLookup = order.Items.ToDictionary(i => i.Id);
        var emailItems = request.Items
            .Select(i => new ProblemReportEmailItem(itemLookup[i.OrderItemId].Widget.Name, i.IssueType))
            .ToList();

        return new ProblemReportEmailMessage(request.OrderId, emailItems, request.Notes);
    }
}