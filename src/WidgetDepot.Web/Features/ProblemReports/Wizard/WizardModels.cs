namespace WidgetDepot.Web.Features.ProblemReports.Wizard;

public class ItemSelectionModel
{
    public int OrderItemId { get; set; }
    public string WidgetName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsSelected { get; set; }
}

public class IssueSpecificationModel
{
    public int OrderItemId { get; set; }
    public string WidgetName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? IssueType { get; set; }
}

internal record SubmitProblemReportItemBody(int OrderItemId, string IssueType);

internal record SubmitProblemReportBody(int OrderId, IReadOnlyList<SubmitProblemReportItemBody> Items, string? Notes);

public abstract record SubmitProblemReportResult
{
    public record Success(int ProblemReportId) : SubmitProblemReportResult;
    public record Failure : SubmitProblemReportResult;
}