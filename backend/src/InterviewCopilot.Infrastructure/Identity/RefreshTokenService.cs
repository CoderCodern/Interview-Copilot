using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using InterviewCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>
/// Owns refresh-token lifecycle (Doc 17 §3.3): generation, SHA-256 hashing, persistence,
/// rotation, reuse-detection, and revocation. Self-contained transaction boundary.
/// </summary>
public sealed class RefreshTokenService(
    AppDbContext db,
    IDateTimeProvider clock,
    IOptions<AuthOptions> options) : IRefreshTokenService
{
    private readonly AuthOptions _options = options.Value;

    public async Task<IssuedRefreshToken> IssueAsync(CandidateId owner, string? userAgent, string? ipHash, CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        var expires = now.AddDays(_options.RefreshTokenDays);
        var raw = TokenHashing.NewOpaqueToken();

        var session = UserSession.Start(owner, TokenHashing.Sha256(raw), expires, now, userAgent, ipHash);
        db.Sessions.Add(session);
        await db.SaveChangesAsync(ct);

        return new IssuedRefreshToken(raw, session.Id, expires);
    }

    public async Task<Result<RotatedRefreshToken>> RotateAsync(string rawToken, string? ipHash, CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        var presentedHash = TokenHashing.Sha256(rawToken);

        var session = await db.Sessions
            .Include(s => s.Tokens)
            .FirstOrDefaultAsync(s => s.Tokens.Any(t => t.TokenHash == presentedHash), ct);

        if (session is null)
        {
            return Error.Validation("auth.refresh_invalid", "Unknown refresh token.");
        }

        var newRaw = TokenHashing.NewOpaqueToken();
        var newExpiry = now.AddDays(_options.RefreshTokenDays);

        var rotation = session.Rotate(presentedHash, TokenHashing.Sha256(newRaw), newExpiry, now, ipHash);

        // Persist either the new token chain (success) or the session revocation (reuse).
        await db.SaveChangesAsync(ct);

        return rotation.IsFailure
            ? rotation.Error
            : new RotatedRefreshToken(newRaw, session.OwnerId, session.Id, newExpiry);
    }

    public async Task RevokeByRawTokenAsync(string rawToken, CancellationToken ct = default)
    {
        var hash = TokenHashing.Sha256(rawToken);
        var session = await db.Sessions
            .Include(s => s.Tokens)
            .FirstOrDefaultAsync(s => s.Tokens.Any(t => t.TokenHash == hash), ct);

        if (session is not null)
        {
            session.Revoke(clock.UtcNow);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeSessionAsync(CandidateId owner, Guid sessionId, CancellationToken ct = default)
    {
        var id = new UserSessionId(sessionId);
        var session = await db.Sessions
            .Include(s => s.Tokens)
            .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == owner, ct);

        if (session is not null)
        {
            session.Revoke(clock.UtcNow);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeAllAsync(CandidateId owner, CancellationToken ct = default)
    {
        var sessions = await db.Sessions
            .Include(s => s.Tokens)
            .Where(s => s.OwnerId == owner && s.RevokedAt == null)
            .ToListAsync(ct);

        var now = clock.UtcNow;
        foreach (var session in sessions)
        {
            session.Revoke(now);
        }

        if (sessions.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<SessionInfo>> ListSessionsAsync(CandidateId owner, string? currentRawToken, CancellationToken ct = default)
    {
        var currentHash = string.IsNullOrEmpty(currentRawToken) ? null : TokenHashing.Sha256(currentRawToken);
        var now = clock.UtcNow;

        var sessions = await db.Sessions
            .Include(s => s.Tokens)
            .Where(s => s.OwnerId == owner && s.RevokedAt == null)
            .OrderByDescending(s => s.LastSeenAt)
            .ToListAsync(ct);

        return
        [
            .. sessions.Select(s => new SessionInfo(
                s.Id.Value,
                s.UserAgent,
                s.CreatedAt,
                s.LastSeenAt,
                currentHash is not null && s.Tokens.Any(t => t.TokenHash == currentHash && t.IsActive(now))))
        ];
    }
}
