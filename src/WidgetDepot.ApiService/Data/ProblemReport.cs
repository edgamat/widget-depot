namespace WidgetDepot.ApiService.Data;

public class ProblemReport
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ProblemReportItem> Items { get; set; } = [];
}