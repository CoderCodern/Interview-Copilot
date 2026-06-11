using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Domain.Users;

/// <summary>
/// A single refresh token within a <see cref="UserSession"/> (Doc 17 §3.3). The domain
/// only ever sees the SHA-256 <see cref="TokenHash"/>; the raw value lives solely in the
/// client cookie. Rotation links the old token to its successor via <see cref="ReplacedById"/>.
/// </summary>
public sealed class RefreshToken : Entity<RefreshTokenId>
{
    internal RefreshToken(
        RefreshTokenId id,
        UserSessionId sessionId,
        CandidateId ownerId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        string? createdIpHash) : base(id)
    {
        SessionId = sessionId;
        OwnerId = ownerId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        CreatedIpHash = createdIpHash;
    }

    public UserSessionId SessionId { get; private set; }
    public CandidateId OwnerId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public RefreshTokenId? ReplacedById { get; private set; }
    public string? CreatedIpHash { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && now < ExpiresAt;

    internal void Revoke(DateTimeOffset now)
    {
        RevokedAt ??= now;
    }

    internal void ReplaceWith(RefreshTokenId successor, DateTimeOffset now)
    {
        Revoke(now);
        ReplacedById = successor;
    }

#pragma warning disable S1144 // EF Core materialization constructor.
    private RefreshToken() : base(default) { TokenHash = null!; }
#pragma warning restore S1144
}
