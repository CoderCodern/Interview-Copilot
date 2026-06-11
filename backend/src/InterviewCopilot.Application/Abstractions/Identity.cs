using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Abstractions;

// ---- Read models passed across the Application boundary (no Identity/EF types leak) ----

/// <summary>A minimal projection of the authenticated user (Doc 17 §3.2).</summary>
public sealed record AuthUser(
    CandidateId Id,
    string Email,
    string FullName,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles,
    string Plan,
    string? AvatarUrl);

/// <summary>A signed access token plus its lifetime (Doc 17 §3.2).</summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt, int ExpiresInSeconds);

/// <summary>A freshly issued refresh token; the raw value is returned exactly once.</summary>
public sealed record IssuedRefreshToken(string RawToken, UserSessionId SessionId, DateTimeOffset ExpiresAt);

/// <summary>The result of rotating a refresh token (Doc 17 §3.3).</summary>
public sealed record RotatedRefreshToken(string RawToken, CandidateId UserId, UserSessionId SessionId, DateTimeOffset ExpiresAt);

/// <summary>A row in the device/session manager (Doc 17 §6).</summary>
public sealed record SessionInfo(Guid Id, string? UserAgent, DateTimeOffset CreatedAt, DateTimeOffset LastSeenAt, bool IsCurrent);

// ---- Ports (implemented in Infrastructure) ----

/// <summary>Issues signed RS256 JWT access tokens (Doc 17 §3.2). JWKS is exposed by the API.</summary>
public interface ITokenService
{
    public AccessToken Issue(AuthUser user, UserSessionId sessionId);
}

/// <summary>
/// Wraps ASP.NET Identity (UserManager/SignInManager/RoleManager) behind a Result-returning
/// port (Doc 17 §1). The implementation self-persists via the Identity stores.
/// </summary>
public interface IIdentityService
{
    public Task<Result<CandidateId>> RegisterAsync(string email, string password, string fullName, CancellationToken ct = default);
    public Task<Result<AuthUser>> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
    public Task<Result<AuthUser>> FindByIdAsync(CandidateId id, CancellationToken ct = default);
    public Task<Result<AuthUser>> FindByEmailAsync(string email, CancellationToken ct = default);
    public Task RecordLoginAsync(CandidateId id, CancellationToken ct = default);

    public Task<Result<string>> GenerateEmailConfirmationTokenAsync(CandidateId id, CancellationToken ct = default);
    public Task<Result> ConfirmEmailAsync(CandidateId id, string token, CancellationToken ct = default);

    public Task<Result<string>> GeneratePasswordResetTokenAsync(CandidateId id, CancellationToken ct = default);
    public Task<Result> ResetPasswordAsync(CandidateId id, string token, string newPassword, CancellationToken ct = default);
    public Task<Result> ChangePasswordAsync(CandidateId id, string currentPassword, string newPassword, CancellationToken ct = default);
}

/// <summary>
/// Owns refresh-token generation, hashing, persistence, rotation, and reuse-detection
/// (Doc 17 §3.3). Self-contained transaction boundary so auth commands stay simple.
/// </summary>
public interface IRefreshTokenService
{
    public Task<IssuedRefreshToken> IssueAsync(CandidateId owner, string? userAgent, string? ipHash, CancellationToken ct = default);
    public Task<Result<RotatedRefreshToken>> RotateAsync(string rawToken, string? ipHash, CancellationToken ct = default);
    public Task RevokeByRawTokenAsync(string rawToken, CancellationToken ct = default);
    public Task RevokeSessionAsync(CandidateId owner, Guid sessionId, CancellationToken ct = default);
    public Task RevokeAllAsync(CandidateId owner, CancellationToken ct = default);
    public Task<IReadOnlyList<SessionInfo>> ListSessionsAsync(CandidateId owner, string? currentRawToken, CancellationToken ct = default);
}

/// <summary>Repository for the locally-owned <see cref="UserProfile"/> aggregate.</summary>
public interface IUserProfileRepository
{
    public Task<UserProfile?> GetAsync(CandidateId id, CancellationToken ct = default);
    public void Add(UserProfile profile);
}

/// <summary>Writes append-only security audit rows (Doc 17 §9). Self-persists.</summary>
public interface IAuthAuditWriter
{
    public Task WriteAsync(AuthAuditLog entry, CancellationToken ct = default);
}

/// <summary>Sends transactional auth emails (Doc 17 §9). SES in prod; logging sink in dev.</summary>
public interface IEmailSender
{
    public Task SendEmailVerificationAsync(string email, string fullName, string verificationLink, CancellationToken ct = default);
    public Task SendPasswordResetAsync(string email, string fullName, string resetLink, CancellationToken ct = default);
}

/// <summary>Builds the user-facing links embedded in auth emails (frontend origin + token).</summary>
public interface IAuthLinkBuilder
{
    public string EmailVerificationLink(CandidateId userId, string token);
    public string PasswordResetLink(string email, string token);
}
