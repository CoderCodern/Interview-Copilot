using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Auth.ResetPassword;

// ---- Command ----
/// <summary>Sets a new password from a reset token and revokes all sessions (Doc 17 §6/§9).</summary>
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand;

// ---- Validator ----
public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Token).NotEmpty();
        RuleFor(c => c.NewPassword).NotEmpty().MinimumLength(PasswordPolicy.MinLength).WithMessage(PasswordPolicy.Description);
    }
}

// ---- Handler ----
public sealed class ResetPasswordHandler(
    IIdentityService identity,
    IRefreshTokenService refresh,
    IAuthAuditWriter audit)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand cmd, CancellationToken cancellationToken)
    {
        var user = await identity.FindByEmailAsync(cmd.Email.Trim(), cancellationToken);
        if (user.IsFailure)
        {
            // Generic failure — do not reveal whether the email exists.
            return Error.Validation("auth.reset_invalid", "The reset link is invalid or has expired.");
        }

        var reset = await identity.ResetPasswordAsync(user.Value.Id, cmd.Token, cmd.NewPassword, cancellationToken);
        if (reset.IsFailure)
        {
            return reset.Error;
        }

        await refresh.RevokeAllAsync(user.Value.Id, cancellationToken);
        await audit.WriteAsync(new AuthAuditLog(user.Value.Id, AuthEvent.PasswordReset), cancellationToken);
        return Result.Success();
    }
}
