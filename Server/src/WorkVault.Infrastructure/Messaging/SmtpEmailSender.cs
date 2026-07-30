using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Messaging;
using WorkVault.Application.Common.Settings;

namespace WorkVault.Infrastructure.Messaging;

/// <summary>
/// Sends email over SMTP (Brevo in production). Provider-agnostic — swapping
/// providers is a config change (host/port/user/key), not a code change.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort);
        client.Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpKey);
        client.EnableSsl = true;

        using var mail = new MailMessage();
        mail.From = new MailAddress(_settings.FromEmail, _settings.FromName);
        mail.Subject = message.Subject;
        mail.Body = message.Body;
        mail.IsBodyHtml = false;  // plaintext for now; HTML template comes next
        
        mail.To.Add(message.To);

        await client.SendMailAsync(mail, cancellationToken);

        _logger.LogInformation("Email sent to {To} (type: {Type})", message.To, message.Type);
    }
}