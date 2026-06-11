using InterviewCopilot.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace InterviewCopilot.Api.Authentication;

/// <summary>
/// Reads/writes the refresh-token cookie (Doc 17 §3.4). Uses the <c>__Host-</c> prefix which
/// the browser only honours with Secure + Path=/ + no Domain, hardening against subdomain and
/// downgrade attacks. SameSite=Strict mitigates CSRF on the refresh endpoint.
/// </summary>
public sealed class RefreshCookie(IOptions<AuthOptions> options)
{
    private readonly AuthOptions _options = options.Value;

    public string? Read(HttpContext ctx) =>
        ctx.Request.Cookies.TryGetValue(_options.RefreshCookieName, out var v) ? v : null;

    public void Write(HttpContext ctx, string rawToken, DateTimeOffset expiresAt) =>
        ctx.Response.Cookies.Append(_options.RefreshCookieName, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = expiresAt,
            IsEssential = true
        });

    public void Clear(HttpContext ctx) =>
        ctx.Response.Cookies.Delete(_options.RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
}
