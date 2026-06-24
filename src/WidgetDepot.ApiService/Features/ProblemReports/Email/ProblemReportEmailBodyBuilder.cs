using System.Text;

namespace WidgetDepot.ApiService.Features.ProblemReports.Email;

public static class ProblemReportEmailBodyBuilder
{
    public static string Build(ProblemReportEmailMessage message)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Order: #{message.OrderId}");
        sb.AppendLine();
        sb.AppendLine("Reported Items:");
        foreach (var item in message.Items)
        {
            sb.AppendLine($"  - {item.WidgetName}: {item.IssueType}");
        }
        if (!string.IsNullOrEmpty(message.Notes))
        {
            sb.AppendLine();
            sb.AppendLine($"Notes: {message.Notes}");
        }
        return sb.ToString();
    }
}