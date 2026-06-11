using System.Security.Cryptography;
using System.Text;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>
/// Opaque-token generation + hashing (Doc 17 §3.3). Refresh tokens are 256 bits of CSPRNG
/// entropy; only their SHA-256 hash is persisted, so a DB leak cannot reconstruct live tokens.
/// Public so the API edge can hash client IPs for audit rows without re-implementing it.
/// </summary>
public static class TokenHashing
{
    public static string NewOpaqueToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    public static string Sha256(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>Salted hash of a client IP — never store raw IPs (Doc 10 §5).</summary>
    public static string? HashIp(string? ip, string salt) =>
        string.IsNullOrWhiteSpace(ip) ? null : Sha256($"{salt}:{ip}");

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

/// <summary>System clock implementation of the <c>IDateTimeProvider</c> port.</summary>
public sealed class SystemDateTimeProvider : Application.Abstractions.IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
