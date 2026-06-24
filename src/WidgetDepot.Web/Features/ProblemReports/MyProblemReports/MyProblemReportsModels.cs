namespace WidgetDepot.Web.Features.ProblemReports.MyProblemReports;

public record ProblemReportListItem(
    int Id,
    int OrderId,
    DateTime OrderSubmittedAt,
    DateTime ReportFiledAt,
    bool EmailSent);

public abstract record GetMyProblemReportsResult
{
    public record Success(IReadOnlyList<ProblemReportListItem> Reports) : GetMyProblemReportsResult;
    public record Failure : GetMyProblemReportsResult;
}

public abstract record ResendEmailResult
{
    public record Success : ResendEmailResult;
    public record Failure : ResendEmailResult;
}

internal record GetMyProblemReportsApiResponse(
    int Id,
    int OrderId,
    DateTime OrderSubmittedAt,
    DateTime ReportFiledAt,
    bool EmailSent);