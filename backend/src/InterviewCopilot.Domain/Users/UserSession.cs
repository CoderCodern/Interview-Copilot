using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Domain.Users;

/// <summary>
/// An authenticated device/session (Doc 17 §3). Aggregate root that owns its chain of
/// rotating <see cref="RefreshToken"/>s and enforces rotation + reuse-detection invariants.
/// One active token per healthy session; presenting a non-active token is treated as theft
/// and revokes the whole session.
/// </summary>
public sealed class UserSession : AggregateRoot<UserSessionId>
{
    private readonly List<RefreshToken> _tokens = [];

    private UserSession(UserSessionId id, CandidateId ownerId, string? userAgent, string? ipHash, DateTimeOffset now)
        : base(id)
    {
        OwnerId = ownerId;
        UserAgent = userAgent;
        IpHash = ipHash;
        CreatedAt = now;
        LastSeenAt = now;
    }

#pragma warning disable S1144 // EF Core materialization constructor.
    private UserSession() : base(default) { }
#pragma warning restore S1144

    public CandidateId OwnerId { get; private set; }
    public string? UserAgent { get; private set; }
    public string? IpHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public IReadOnlyList<RefreshToken> Tokens => _tokens.AsReadOnly();
    public bool IsActive => RevokedAt is null;

    /// <summary>Opens a session and issues its first refresh token.</summary>
    public static UserSession Start(
        CandidateId ownerId,
        string tokenHash,
        DateTimeOffset expiresAt,
        DateTimeOffset now,
        string? userAgent = null,
        string? ipHash = null)
    {
        var session = new UserSession(UserSessionId.New(), ownerId, userAgent, ipHash, now);
        session._tokens.Add(new RefreshToken(
            RefreshTokenId.New(), session.Id, ownerId, tokenHash, now, expiresAt, ipHash));
        return session;
    }

    /// <summary>
    /// Rotates the presented refresh token: revokes it and issues a successor. If the
    /// presented hash is unknown, expired, or already-revoked (replay/theft), the entire
    /// session is revoked and a failure is returned (Doc 17 §3.3).
    /// </summary>
    public Result<RefreshToken> Rotate(string presentedHash, string newHash, DateTimeOffset newExpiry, DateTimeOffset now, string? ipHash = null)
    {
        if (!IsActive)
        {
            return Error.Validation("auth.refresh_invalid", "Session is no longer active.");
        }

        var presented = _tokens.Find(t => t.TokenHash == presentedHash);

        if (presented is null)
        {
            return Error.Validation("auth.refresh_invalid", "Unknown refresh token.");
        }

        // Reuse/theft: a non-active (revoked or expired) token was presented → burn the session.
        if (!presented.IsActive(now))
        {
            RevokeInternal(now);
            return Error.Validation("auth.refresh_reused", "Refresh token was already used or expired; session revoked.");
        }

        var successor = new RefreshToken(RefreshTokenId.New(), Id, OwnerId, newHash, now, newExpiry, ipHash);
        presented.ReplaceWith(successor.Id, now);
        _tokens.Add(successor);
        LastSeenAt = now;
        return successor;
    }

    /// <summary>Revokes this session (logout of one device).</summary>
    public void Revoke(DateTimeOffset now) => RevokeInternal(now);

    private void RevokeInternal(DateTimeOffset now)
    {
        if (RevokedAt is null)
        {
            RevokedAt = now;
        }

        foreach (var token in _tokens)
        {
            token.Revoke(now);
        }
    }

}
