using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.ProblemReports.Email;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.ProblemReports.ResendProblemReportEmail;

public record ResendProblemReportEmailCommand(int ProblemReportId, int CustomerId) : IRequest<object>;

public record ResendProblemReportEmailSuccess;

public record ResendProblemReportEmailNotFound;

public record ResendProblemReportEmailFailed;

public class ResendProblemReportEmailHandler : IRequestHandler<ResendProblemReportEmailCommand, object>
{
    private readonly AppDbContext _db;
    private readonly IProblemReportEmailSender _emailSender;

    public ResendProblemReportEmailHandler(AppDbContext db, IProblemReportEmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender;
    }

    public async Task<object> HandleAsync(ResendProblemReportEmailCommand command, CancellationToken cancellationToken)
    {
        var report = await _db.ProblemReports
            .Include(pr => pr.Items)
            .ThenInclude(i => i.OrderItem)
            .ThenInclude(oi => oi.Widget)
            .Include(pr => pr.Order)
            .FirstOrDefaultAsync(pr => pr.Id == command.ProblemReportId && pr.Order.CustomerId == command.CustomerId, cancellationToken);

        if (report is null)
            return new ResendProblemReportEmailNotFound();

        var emailItems = report.Items
            .Select(i => new ProblemReportEmailItem(i.OrderItem.Widget.Name, i.IssueType.ToString()))
            .ToList();

        var message = new ProblemReportEmailMessage(report.OrderId, emailItems, report.Notes);
        var sent = await _emailSender.SendAsync(message, cancellationToken);

        if (!sent)
            return new ResendProblemReportEmailFailed();

        report.EmailSent = true;
        await _db.SaveChangesAsync(cancellationToken);

        return new ResendProblemReportEmailSuccess();
    }
}