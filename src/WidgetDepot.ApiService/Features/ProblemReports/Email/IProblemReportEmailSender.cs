namespace WidgetDepot.ApiService.Features.ProblemReports.Email;

public interface IProblemReportEmailSender
{
    Task<bool> SendAsync(ProblemReportEmailMessage message, CancellationToken cancellationToken);
}

public record ProblemReportEmailMessage(
    int OrderId,
    IReadOnlyList<ProblemReportEmailItem> Items,
    string? Notes);

public record ProblemReportEmailItem(string WidgetName, string IssueType);