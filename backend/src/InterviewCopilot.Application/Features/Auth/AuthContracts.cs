using InterviewCopilot.Application.Abstractions;

namespace InterviewCopilot.Application.Features.Auth;

/// <summary>User projection returned in auth responses (Doc 17 §6.2).</summary>
public sealed record AuthUserDto(
    Guid Id,
    string Email,
    string FullName,
    bool EmailConfirmed,
    string[] Roles,
    string Plan,
    string? AvatarUrl)
{
    public static AuthUserDto From(AuthUser u) =>
        new(u.Id.Value, u.Email, u.FullName, u.EmailConfirmed, [.. u.Roles], u.Plan, u.AvatarUrl);
}

/// <summary>
/// The full result of an authenticating command (login / refresh / google). The API endpoint
/// places <see cref="RefreshToken"/> in a <c>__Host-</c> cookie and returns the rest as JSON
/// (Doc 17 §3.4); native clients receive the refresh token in the body.
/// </summary>
public sealed record AuthResult(
    string AccessToken,
    int ExpiresIn,
    AuthUserDto User,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt)
{
    public const string TokenType = "Bearer";
}

/// <summary>Centralized password policy text (mirrors Identity options, Doc 17 §9).</summary>
public static class PasswordPolicy
{
    public const int MinLength = 10;
    public const string Description =
        "Password must be at least 10 characters and include upper, lower, and digit characters.";
}
