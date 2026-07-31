namespace WorkVault.Application.Common.Messaging;

/// <summary>
/// A "fat" email message — carries everything the consumer needs to send the
/// email, so the consumer needs no database lookup. Serialized to JSON and
/// published to the email queue.
/// </summary>
public record EmailMessage(
    string To,
    string Subject,
    string Body,
    EmailType Type
);

public enum EmailType
{
    Registration,
    Invite,
    PasswordReset
}