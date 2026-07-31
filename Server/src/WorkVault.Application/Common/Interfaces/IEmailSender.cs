using WorkVault.Application.Common.Messaging;

namespace WorkVault.Application.Common.Interfaces;

/// <summary>
/// Sends an email. This is the "actually deliver it" side — called by the
/// consumer, not by request handlers. Handlers publish; the consumer sends.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}