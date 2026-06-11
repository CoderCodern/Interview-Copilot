namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>
/// Strongly-typed JWT/auth configuration (CLAUDE.md §9 — secrets via env/Secrets Manager).
/// Bound from the <c>Auth</c> configuration section.
/// </summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>JWT issuer (e.g. https://api.interviewcopilot.ai).</summary>
    public string Issuer { get; init; } = "https://api.interviewcopilot.ai";

    /// <summary>JWT audience.</summary>
    public string Audience { get; init; } = "interview-copilot";

    /// <summary>Access-token lifetime in minutes (Doc 17: 15).</summary>
    public int AccessTokenMinutes { get; init; } = 15;

    /// <summary>Refresh-token lifetime in days (Doc 17: 30).</summary>
    public int RefreshTokenDays { get; init; } = 30;

    /// <summary>RSA private key (PEM) used to sign access tokens. Sourced from Secrets Manager
    /// in production; an ephemeral dev key is generated if absent (logged warning).</summary>
    public string? SigningKeyPem { get; init; }

    /// <summary>Stable key id published in the JWT header and JWKS.</summary>
    public string SigningKeyId { get; init; } = "ic-auth-1";

    /// <summary>Salt mixed into IP-address hashing for audit rows (PII minimization).</summary>
    public string IpHashSalt { get; init; } = "change-me";

    /// <summary>Frontend origin used to build email links (verify/reset).</summary>
    public string WebOrigin { get; init; } = "http://localhost:3000";

    /// <summary>Cookie name for the refresh token. <c>__Host-</c> prefix requires Secure + Path=/.</summary>
    public string RefreshCookieName { get; init; } = "__Host-ic_refresh";
}

/// <summary>Google OAuth settings (Doc 17 §6.4). Used by the P3 Google slices.</summary>
public sealed class GoogleOAuthOptions
{
    public const string SectionName = "Auth:Google";
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public string RedirectUri { get; init; } = "http://localhost:8080/api/v1/auth/google/callback";
}
