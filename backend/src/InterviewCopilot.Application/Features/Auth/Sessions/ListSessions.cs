using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Application.Features.Auth.Sessions;

// ---- Response ----
public sealed record SessionDto(Guid Id, string? UserAgent, DateTimeOffset CreatedAt, DateTimeOffset LastSeenAt, bool IsCurrent);

// ---- Query ----
/// <summary>Lists the current user's active sessions for the device manager (Doc 17 §6).</summary>
public sealed record ListSessionsQuery(string? CurrentRefreshToken) : IQuery<IReadOnlyList<SessionDto>>;

// ---- Handler ----
public sealed class ListSessionsHandler(IRefreshTokenService refresh, ICurrentUser currentUser)
    : IQueryHandler<ListSessionsQuery, IReadOnlyList<SessionDto>>
{
    public async Task<Result<IReadOnlyList<SessionDto>>> Handle(ListSessionsQuery query, CancellationToken cancellationToken)
    {
        var sessions = await refresh.ListSessionsAsync(currentUser.Id, query.CurrentRefreshToken, cancellationToken);
        IReadOnlyList<SessionDto> dtos =
            [.. sessions.Select(s => new SessionDto(s.Id, s.UserAgent, s.CreatedAt, s.LastSeenAt, s.IsCurrent))];
        return Result.Success(dtos);
    }
}
