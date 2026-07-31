using System.Reflection;

namespace WorkVault.Infrastructure.Messaging;

/// <summary>
/// Static email assets embedded in this assembly. Emails inline the brand mark as a
/// <c>cid:</c> attachment, so it renders in Gmail/Outlook without depending on the
/// separately-deployed frontend/CDN being reachable at a known URL.
/// </summary>
public static class EmailAssets
{
    /// <summary>
    /// Content-Id the email HTML references: <c>&lt;img src="cid:workvault-logo"&gt;</c>.
    /// Must match the value used in <c>EmailTemplate</c>.
    /// </summary>
    public const string LogoContentId = "workvault-logo";

    /// <summary>The brand-mark PNG bytes, loaded once from the embedded resource (null if missing).</summary>
    public static readonly byte[]? LogoBytes = LoadEmbedded("workvault-mark.png");

    private static byte[]? LoadEmbedded(string fileNameSuffix)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileNameSuffix, StringComparison.OrdinalIgnoreCase));
        if (resourceName is null)
            return null;

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return null;

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
