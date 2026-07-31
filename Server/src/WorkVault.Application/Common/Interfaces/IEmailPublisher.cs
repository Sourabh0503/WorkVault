using WorkVault.Application.Common.Messaging;

namespace WorkVault.Application.Common.Interfaces;

/// <summary>
/// Publishes email jobs to the message queue. The actual sending happens
/// later, in a separate consumer — this only enqueues.
/// </summary>
public interface IEmailPublisher
{
    Task PublishAsync(EmailMessage message, CancellationToken cancellationToken = default);
}