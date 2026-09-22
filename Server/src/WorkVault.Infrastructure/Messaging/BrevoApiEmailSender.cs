using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Messaging;
using WorkVault.Application.Common.Settings;

namespace WorkVault.Infrastructure.Messaging;

/// <summary>
/// Sends email via Brevo's transactional HTTP API (POST https://api.brevo.com/v3/smtp/email).
/// Uses HTTPS (443) instead of SMTP, so it works on hosts that block outbound SMTP ports
/// (e.g. Render's free plan blocks 25/465/587). The brand mark is referenced by hosted URL
/// in the HTML — no inline attachment needed.
/// </summary>
public class BrevoApiEmailSender : IEmailSender
{
    private const string SendEndpoint = "https://api.brevo.com/v3/smtp/email";

    private readonly HttpClient _http;
    private readonly EmailSettings _settings;
    private readonly ILogger<BrevoApiEmailSender> _logger;

    public BrevoApiEmailSender(HttpClient http, IOptions<EmailSettings> settings, ILogger<BrevoApiEmailSender> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var payload = new BrevoSendRequest
        {
            Sender = new BrevoContact { Name = _settings.FromName, Email = _settings.FromEmail },
            To = new[] { new BrevoContact { Email = message.To } },
            Subject = message.Subject,
            HtmlContent = message.Body
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, SendEndpoint)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("api-key", _settings.ApiKey);

        using var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Brevo API returned {(int)response.StatusCode} {response.ReasonPhrase} sending email to {message.To}: {body}");
        }

        _logger.LogInformation("Email sent to {To} (type: {Type})", message.To, message.Type);
    }

    // Brevo API request shapes (camelCase to match the API).
    private sealed class BrevoSendRequest
    {
        [JsonPropertyName("sender")] public BrevoContact Sender { get; set; } = null!;
        [JsonPropertyName("to")] public BrevoContact[] To { get; set; } = null!;
        [JsonPropertyName("subject")] public string Subject { get; set; } = string.Empty;
        [JsonPropertyName("htmlContent")] public string HtmlContent { get; set; } = string.Empty;
    }

    private sealed class BrevoContact
    {
        [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }
    }
}
