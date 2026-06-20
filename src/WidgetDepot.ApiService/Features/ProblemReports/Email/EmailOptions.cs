namespace WidgetDepot.ApiService.Features.ProblemReports.Email;

public class EmailOptions
{
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string FromAddress { get; set; } = "noreply@widgetdepot.local";
    public string WarehouseStaffAddress { get; set; } = "";
}