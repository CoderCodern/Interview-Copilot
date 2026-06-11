using System.Security.Claims;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>Issues RS256 JWT access tokens with the claim set from Doc 17 §3.2.</summary>
public sealed class TokenService(
    IOptions<AuthOptions> options,
    RsaSigningKeyProvider keys,
    IDateTimeProvider clock) : ITokenService
{
    private readonly AuthOptions _options = options.Value;

    public AccessToken Issue(AuthUser user, UserSessionId sessionId)
    {
        var now = clock.UtcNow;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("email_verified", user.EmailConfirmed ? "true" : "false", ClaimValueTypes.Boolean),
            new("name", user.FullName),
            new("plan", user.Plan),
            new("sid", sessionId.Value.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = keys.SigningCredentials
        };

        var handler = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false };
        var jwt = handler.CreateToken(descriptor);

        return new AccessToken(jwt, expires, _options.AccessTokenMinutes * 60);
    }
}
