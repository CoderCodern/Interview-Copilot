using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Auth.LogoutAll;

// ---- Command ----
/// <summary>Revokes every session for the current user — "log out everywhere" (Doc 17 §3.3).</summary>
public sealed record LogoutAllCommand : ICommand;

// ---- Handler ----
public sealed class LogoutAllHandler(
    IRefreshTokenService refresh,
    ICurrentUser currentUser,
    IAuthAuditWriter audit)
    : ICommandHandler<LogoutAllCommand>
{
    public async Task<Result> Handle(LogoutAllCommand cmd, CancellationToken cancellationToken)
    {
        await refresh.RevokeAllAsync(currentUser.Id, cancellationToken);
        await audit.WriteAsync(new AuthAuditLog(currentUser.Id, AuthEvent.LoggedOutAll), cancellationToken);
        return Result.Success();
    }
}
