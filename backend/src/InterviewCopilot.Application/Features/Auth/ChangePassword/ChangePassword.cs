using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Auth.ChangePassword;

// ---- Command ----
/// <summary>Changes the password for the signed-in user and revokes all other sessions
/// (Doc 17 §6/§9). The endpoint re-issues a fresh session for the current device.</summary>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand;

// ---- Validator ----
public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(c => c.CurrentPassword).NotEmpty();
        RuleFor(c => c.NewPassword).NotEmpty().MinimumLength(PasswordPolicy.MinLength).WithMessage(PasswordPolicy.Description)
            .NotEqual(c => c.CurrentPassword).WithMessage("New password must differ from the current one.");
    }
}

// ---- Handler ----
public sealed class ChangePasswordHandler(
    IIdentityService identity,
    IRefreshTokenService refresh,
    ICurrentUser currentUser,
    IAuthAuditWriter audit)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand cmd, CancellationToken cancellationToken)
    {
        var changed = await identity.ChangePasswordAsync(currentUser.Id, cmd.CurrentPassword, cmd.NewPassword, cancellationToken);
        if (changed.IsFailure)
        {
            return changed.Error;
        }

        await refresh.RevokeAllAsync(currentUser.Id, cancellationToken);
        await audit.WriteAsync(new AuthAuditLog(currentUser.Id, AuthEvent.PasswordChanged), cancellationToken);
        return Result.Success();
    }
}
