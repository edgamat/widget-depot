namespace WidgetDepot.ApiService.Data;

public class ProblemReportItem
{
    public int Id { get; set; }
    public int ProblemReportId { get; set; }
    public int OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;
    public IssueType IssueType { get; set; }
}