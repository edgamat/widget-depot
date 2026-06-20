using System.Text;

using MailKit.Net.Smtp;

using Microsoft.Extensions.Options;

using MimeKit;

namespace WidgetDepot.ApiService.Features.ProblemReports.Email;

public class MailKitProblemReportEmailSender : IProblemReportEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<MailKitProblemReportEmailSender> _logger;

    public MailKitProblemReportEmailSender(
        IOptions<EmailOptions> options,
        ILogger<MailKitProblemReportEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(ProblemReportEmailMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_options.FromAddress));
            email.To.Add(MailboxAddress.Parse(_options.WarehouseStaffAddress));
            email.Subject = $"Problem Report for Order #{message.OrderId}";
            email.Body = new TextPart("plain") { Text = BuildEmailBody(message) };

            using var client = new SmtpClient();
            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, MailKit.Security.SecureSocketOptions.None, cancellationToken);
            await client.SendAsync(email, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send problem report email for order {OrderId}", message.OrderId);
            return false;
        }
    }

    private static string BuildEmailBody(ProblemReportEmailMessage message)
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