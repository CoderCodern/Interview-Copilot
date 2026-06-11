using InterviewCopilot.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InterviewCopilot.Api.Authentication;

/// <summary>
/// Configures JWT bearer validation against our own RS256 signing key (Doc 17 §3.2). Used in
/// every environment now that auth is first-party (ADR 0005) — no dev bypass.
/// </summary>
public sealed class JwtBearerSetup(IOptions<AuthOptions> authOptions, RsaSigningKeyProvider keys)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthOptions _auth = authOptions.Value;

    public void Configure(string? name, JwtBearerOptions options) => Configure(options);

    public void Configure(JwtBearerOptions options)
    {
        options.MapInboundClaims = false; // keep "sub"/"role" as-is
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _auth.Issuer,
            ValidateAudience = true,
            ValidAudience = _auth.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = keys.PublicKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub",
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    }
}
