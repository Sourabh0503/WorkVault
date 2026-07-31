using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
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
        
        mail.To.Add(message.To);

        // Branded HTML body, with the brand mark embedded inline (cid:) so it renders
        // in Gmail/Outlook without depending on the separately-hosted frontend/CDN.
        var htmlView = AlternateView.CreateAlternateViewFromString(
            message.Body, Encoding.UTF8, MediaTypeNames.Text.Html);

        if (EmailAssets.LogoBytes is { } logoBytes
            && message.Body.Contains($"cid:{EmailAssets.LogoContentId}", StringComparison.Ordinal))
        {
            var logo = new LinkedResource(new MemoryStream(logoBytes), MediaTypeNames.Image.Png)
            {
                ContentId = EmailAssets.LogoContentId,
                TransferEncoding = TransferEncoding.Base64
            };
            htmlView.LinkedResources.Add(logo);
        }

        mail.AlternateViews.Add(htmlView);

        await client.SendMailAsync(mail, cancellationToken);

        _logger.LogInformation("Email sent to {To} (type: {Type})", message.To, message.Type);
    }
}