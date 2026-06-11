using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Auth.RefreshSession;

// ---- Command ----
/// <summary>Rotates a refresh token and mints a new access token (Doc 17 §3.3/§6.3). On replay
/// the session chain is revoked and a security event is logged.</summary>
public sealed record RefreshSessionCommand(string RefreshToken, string? IpHash) : ICommand<AuthResult>;

// ---- Validator ----
public sealed class RefreshSessionValidator : AbstractValidator<RefreshSessionCommand>
{
    public RefreshSessionValidator() => RuleFor(c => c.RefreshToken).NotEmpty();
}

// ---- Handler ----
public sealed class RefreshSessionHandler(
    IRefreshTokenService refresh,
    IIdentityService identity,
    ITokenService tokens,
    IAuthAuditWriter audit)
    : ICommandHandler<RefreshSessionCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(RefreshSessionCommand cmd, CancellationToken cancellationToken)
    {
        var rotated = await refresh.RotateAsync(cmd.RefreshToken, cmd.IpHash, cancellationToken);
        if (rotated.IsFailure)
        {
            if (rotated.Error.Code == "auth.refresh_reused")
            {
                await audit.WriteAsync(new AuthAuditLog(null, AuthEvent.RefreshReuseDetected, cmd.IpHash), cancellationToken);
            }

            return rotated.Error;
        }

        var user = await identity.FindByIdAsync(rotated.Value.UserId, cancellationToken);
        if (user.IsFailure)
        {
            return user.Error;
        }

        var access = tokens.Issue(user.Value, rotated.Value.SessionId);
        return new AuthResult(access.Value, access.ExpiresInSeconds, AuthUserDto.From(user.Value),
            rotated.Value.RawToken, rotated.Value.ExpiresAt);
    }
}
