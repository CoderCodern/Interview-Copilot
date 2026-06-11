using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Application.Features.Auth.Sessions;

// ---- Command ----
/// <summary>Revokes one specific session of the current user (Doc 17 §6).</summary>
public sealed record RevokeSessionCommand(Guid SessionId) : ICommand;

// ---- Validator ----
public sealed class RevokeSessionValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionValidator() => RuleFor(c => c.SessionId).NotEmpty();
}

// ---- Handler ----
public sealed class RevokeSessionHandler(IRefreshTokenService refresh, ICurrentUser currentUser)
    : ICommandHandler<RevokeSessionCommand>
{
    public async Task<Result> Handle(RevokeSessionCommand cmd, CancellationToken cancellationToken)
    {
        await refresh.RevokeSessionAsync(currentUser.Id, cmd.SessionId, cancellationToken);
        return Result.Success();
    }
}
