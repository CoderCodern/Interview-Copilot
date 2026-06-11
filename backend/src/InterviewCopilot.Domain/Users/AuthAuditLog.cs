using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Domain.Users;

/// <summary>Security-relevant auth events (Doc 17 §5.2/§9). Append-only.</summary>
public enum AuthEvent
{
    LoginSucceeded = 0,
    LoginFailed = 1,
    Registered = 2,
    EmailVerified = 3,
    PasswordResetRequested = 4,
    PasswordReset = 5,
    PasswordChanged = 6,
    RefreshReuseDetected = 7,
    LoggedOut = 8,
    LoggedOutAll = 9,
    ExternalLoginLinked = 10,
    RoleChanged = 11,
    AccountLocked = 12
}

/// <summary>
/// An immutable audit row. <see cref="UserId"/> is null for failed attempts against an
/// unknown email. Raw IP is never stored — only a salted hash (PII minimization, Doc 10 §5).
/// </summary>
public sealed class AuthAuditLog : Entity<AuthAuditLogId>
{
    public AuthAuditLog(
        CandidateId? userId,
        AuthEvent @event,
        string? ipHash = null,
        string? userAgent = null,
        string? metadata = null) : base(AuthAuditLogId.New())
    {
        UserId = userId;
        Event = @event;
        IpHash = ipHash;
        UserAgent = userAgent;
        Metadata = metadata;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public CandidateId? UserId { get; private set; }
    public AuthEvent Event { get; private set; }
    public string? IpHash { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Metadata { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

#pragma warning disable S1144 // EF Core materialization constructor.
    private AuthAuditLog() : base(default) { }
#pragma warning restore S1144
}
