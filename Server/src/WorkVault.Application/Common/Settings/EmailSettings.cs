namespace WorkVault.Application.Common.Settings;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpKey { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;      // Brevo HTTP API key (xkeysib-…); used by BrevoApiEmailSender
    public string FromEmail { get; set; } = string.Empty;   // your verified Brevo sender
    public string FromName { get; set; } = "WorkVault";
}