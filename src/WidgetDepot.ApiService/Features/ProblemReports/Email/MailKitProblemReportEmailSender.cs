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
            email.Body = new TextPart("plain") { Text = ProblemReportEmailBodyBuilder.Build(message) };

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
}