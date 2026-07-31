using System.Net;

namespace WorkVault.Application.Common.Messaging;

/// <summary>
/// Builds branded, email-client-safe HTML bodies for WorkVault emails.
/// </summary>
/// <remarks>
/// Table-based layout with inline styles — the only reliable approach across
/// Gmail / Outlook / Apple Mail. Markup mirrors the design templates in
/// <c>docs/emails/*.html</c>. All caller-supplied values are HTML-encoded so
/// names / company / email can't break the markup.
/// </remarks>
public static class EmailTemplate
{
    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    /// <summary>Register / verify-email confirmation (see <c>emails/verify-company.html</c>).</summary>
    public static string VerifyEmail(string firstName, string companyName, string ctaUrl, string expiryText)
    {
        var name = Enc(firstName);
        var company = Enc(companyName);

        var topBody = $"""
            <tr><td style="font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#334155;line-height:22px;padding-bottom:6px;">Hi {name},</td></tr>
            <tr><td style="font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#334155;line-height:22px;">Your workspace <strong style="color:#04241B;">{company}</strong> has been created. Confirm your email address to activate it and start inviting your team.</td></tr>
            """;

        var cta = $"""
            {Button("Verify email address", ctaUrl)}
            {Note(expiryText)}
            """;

        return Shell(
            preheader: $"Confirm your email to activate the {companyName} workspace on WorkVault.",
            kicker: "Workspace created",
            headingHtml: "Welcome to WorkVault,<br>Verify your email to begin",
            topBody: topBody,
            ctaBody: cta,
            fallbackUrl: ctaUrl);
    }

    /// <summary>Employee invite (see <c>emails/hr-invite.html</c>).</summary>
    public static string Invite(
        string companyName, string roleName, string? department,
        string employeeCode, string ctaUrl, string expiryText,
        string? inviterName = null)
    {
        var company = Enc(companyName);

        // Name the inviter when we have it ("Sourabh Agrawal has invited you…"),
        // otherwise fall back to a neutral phrasing.
        var intro = string.IsNullOrWhiteSpace(inviterName)
            ? $"""You've been invited to join the <strong style="color:#04241B;">{company}</strong> workspace on WorkVault."""
            : $"""<strong style="color:#04241B;">{Enc(inviterName)}</strong> has invited you to join the <strong style="color:#04241B;">{company}</strong> workspace on WorkVault.""";

        var topBody = $"""
            <tr><td style="font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#334155;line-height:22px;padding-bottom:20px;">{intro}</td></tr>
            {InfoBox(roleName, department, employeeCode)}
            """;

        var cta = $"""
            {Button("Accept invitation", ctaUrl)}
            {Note(expiryText)}
            """;

        return Shell(
            preheader: $"You've been invited to join {companyName} on WorkVault. Accept to set up your account.",
            kicker: "Team invitation",
            headingHtml: $"You've been invited to join {company}",
            topBody: topBody,
            ctaBody: cta,
            fallbackUrl: ctaUrl);
    }

    /// <summary>Password reset (see <c>emails/forgot-password.html</c>).</summary>
    public static string PasswordReset(string firstName, string email, string ctaUrl, string expiryText)
    {
        var name = Enc(firstName);
        var mail = Enc(email);

        var topBody = $"""
            <tr><td style="font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#334155;line-height:22px;padding-bottom:6px;">Hi {name},</td></tr>
            <tr><td style="font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#334155;line-height:22px;padding-bottom:20px;">We received a request to reset the password for your WorkVault account <strong style="color:#04241B;">{mail}</strong>. Click below to choose a new one.</td></tr>
            <tr><td>
              <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" bgcolor="#FFFBEB" style="background-color:#FFFBEB;border:1px solid #FDE9C8;border-radius:12px;">
                <tr><td style="padding:14px 18px;font-family:Arial,Helvetica,sans-serif;font-size:13px;color:#92600D;line-height:20px;"><strong>Didn't request this?</strong> Your password is still safe — no changes were made. You can ignore this email, or contact your workspace admin if you're concerned.</td></tr>
              </table>
            </td></tr>
            """;

        var cta = $"""
            {Button("Reset password", ctaUrl)}
            {Note(expiryText)}
            """;

        return Shell(
            preheader: "We received a request to reset your WorkVault password.",
            kicker: "Security",
            headingHtml: "Reset your password",
            topBody: topBody,
            ctaBody: cta,
            fallbackUrl: ctaUrl);
    }

    // ---- Building blocks ----

    private static string Button(string label, string url) => $"""
        <tr><td align="center" style="padding-bottom:6px;">
          <table role="presentation" cellpadding="0" cellspacing="0" border="0"><tr>
            <td bgcolor="#059669" style="background-color:#059669;border-radius:10px;">
              <a href="{Enc(url)}" style="display:block;padding:12px 30px;font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:bold;color:#FFFFFF;text-decoration:none;">{Enc(label)}</a>
            </td>
          </tr></table>
        </td></tr>
        """;

    private static string Note(string text) => $"""
        <tr><td align="center" style="font-family:Arial,Helvetica,sans-serif;font-size:13px;color:#64748B;line-height:20px;padding-top:12px;">{Enc(text)}</td></tr>
        """;

    /// <summary>Fallback "copy this link" note — pinned to the bottom of the card by the shell.</summary>
    private static string FallbackInner(string url) => $"""
        <div style="border-top:1px solid #E4E9F0;padding-top:16px;font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#94A3B8;line-height:19px;">If the button doesn't work, copy and paste this link into your browser:<br><a href="{Enc(url)}" style="color:#059669;text-decoration:underline;word-break:break-all;">{Enc(url)}</a></div>
        """;

    private static string InfoBox(string roleName, string? department, string employeeCode)
    {
        var deptCell = string.IsNullOrWhiteSpace(department)
            ? string.Empty
            : $"""<td style="padding:12px 16px;font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#64748B;line-height:18px;">Department<br><span style="font-size:14px;font-weight:bold;color:#04241B;line-height:22px;">{Enc(department)}</span></td>""";

        return $"""
            <tr><td>
              <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" bgcolor="#F6FEFB" style="background-color:#F6FEFB;border:1px solid #D1FAE5;border-radius:12px;">
                <tr>
                  <td style="padding:12px 16px;font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#64748B;line-height:18px;">Role<br><span style="font-size:14px;font-weight:bold;color:#04241B;line-height:22px;">{Enc(roleName)}</span></td>
                  {deptCell}
                  <td style="padding:12px 16px;font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#64748B;line-height:18px;">Employee code<br><span style="font-size:14px;font-weight:bold;color:#04241B;line-height:22px;">{Enc(employeeCode)}</span></td>
                </tr>
              </table>
            </td></tr>
            """;
    }

    /// <summary>
    /// The WorkVault brand mark + wordmark. The mark is referenced as an inline
    /// <c>cid:</c> attachment that the SMTP sender embeds (see <c>EmailAssets.LogoContentId</c>),
    /// so it renders in Gmail/Outlook with no external URL. The wordmark text shows even
    /// if images are blocked.
    /// </summary>
    private const string LogoMark =
        """
        <table role="presentation" cellpadding="0" cellspacing="0" border="0"><tr>
          <td valign="middle" style="line-height:0;font-size:0;">
            <img src="cid:workvault-logo" width="34" height="34" alt="WorkVault" style="display:block;border:0;outline:none;text-decoration:none;width:34px;height:34px;">
          </td>
          <td valign="middle" style="padding-left:10px;font-family:Arial,Helvetica,sans-serif;font-size:18px;font-weight:bold;letter-spacing:-0.02em;"><span style="color:#04241B;">work</span><span style="color:#059669;">Vault</span></td>
        </tr></table>
        """;

    /// <summary>
    /// Common shell: fixed 600px content column (natural height — no viewport units,
    /// so it never forces scroll), logo header, dark title band, white card
    /// (text, button, then fallback link stacked), footer.
    /// </summary>
    private static string Shell(
        string preheader, string kicker, string headingHtml,
        string topBody, string ctaBody, string fallbackUrl) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="color-scheme" content="light dark">
        </head>
        <body style="margin:0;padding:0;background-color:#F1F5F4;">
        <span style="display:none;font-size:1px;color:#F1F5F4;line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;">{Enc(preheader)}</span>
        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="background-color:#F1F5F4;">
        <tr><td align="center" valign="top" style="padding:24px 16px;">
        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="600" style="width:600px;max-width:600px;">
          <tr><td style="padding:0 4px 14px;">
            {LogoMark}
          </td></tr>
          <tr><td bgcolor="#04241B" style="background-color:#04241B;border-radius:16px 16px 0 0;padding:26px 30px 22px;">
            <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
              <tr><td style="font-family:Arial,Helvetica,sans-serif;font-size:11px;font-weight:bold;letter-spacing:2px;color:#34D399;text-transform:uppercase;padding-bottom:10px;">{Enc(kicker)}</td></tr>
              <tr><td style="font-family:Arial,Helvetica,sans-serif;font-size:22px;font-weight:bold;color:#FFFFFF;line-height:28px;letter-spacing:-0.02em;">{headingHtml}</td></tr>
            </table>
          </td></tr>
          <tr><td valign="top" bgcolor="#FFFFFF" style="background-color:#FFFFFF;border-radius:0 0 16px 16px;padding:26px 30px 24px;">
            <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
              {topBody}
              <tr><td style="font-size:0;line-height:28px;height:28px;">&nbsp;</td></tr>
              {ctaBody}
              <tr><td style="padding-top:24px;">
                {FallbackInner(fallbackUrl)}
              </td></tr>
            </table>
          </td></tr>
          <tr><td style="padding:16px 30px;font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#94A3B8;line-height:19px;" align="center">
            Crafted with <span style="color:#059669;">&#9829;</span> by <a href="https://www.linkedin.com/in/sourabhagrawal2002/" target="_blank" rel="noopener" style="color:#059669;text-decoration:underline;font-weight:bold;">Sourabh Agrawal</a>
          </td></tr>
        </table>
        </td></tr>
        </table>
        </body>
        </html>
        """;
}
