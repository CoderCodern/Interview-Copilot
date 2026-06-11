using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Auth.Logout;

// ---- Command ----
/// <summary>Revokes the current device's session (Doc 17 §3.3). The raw refresh token comes
/// from the cookie; if absent the call still succeeds (idempotent logout).</summary>
public sealed record LogoutCommand(string? RefreshToken) : ICommand;

// ---- Handler ----
public sealed class LogoutHandler(
    IRefreshTokenService refresh,
    ICurrentUser currentUser,
    IAuthAuditWriter audit)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand cmd, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(cmd.RefreshToken))
        {
            await refresh.RevokeByRawTokenAsync(cmd.RefreshToken, cancellationToken);
        }

        if (currentUser.IsAuthenticated)
        {
            await audit.WriteAsync(new AuthAuditLog(currentUser.Id, AuthEvent.LoggedOut), cancellationToken);
        }

        return Result.Success();
    }
}
