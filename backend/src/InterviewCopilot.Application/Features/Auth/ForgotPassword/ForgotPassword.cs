using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Auth.ForgotPassword;

// ---- Command ----
/// <summary>Sends a password-reset link. Always succeeds (no enumeration, Doc 17 §9).</summary>
public sealed record ForgotPasswordCommand(string Email) : ICommand;

// ---- Validator ----
public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator() => RuleFor(c => c.Email).NotEmpty().EmailAddress();
}

// ---- Handler ----
public sealed class ForgotPasswordHandler(
    IIdentityService identity,
    IEmailSender email,
    IAuthLinkBuilder links,
    IAuthAuditWriter audit)
    : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(ForgotPasswordCommand cmd, CancellationToken cancellationToken)
    {
        var user = await identity.FindByEmailAsync(cmd.Email.Trim(), cancellationToken);
        if (user.IsSuccess)
        {
            var token = await identity.GeneratePasswordResetTokenAsync(user.Value.Id, cancellationToken);
            if (token.IsSuccess)
            {
                var link = links.PasswordResetLink(user.Value.Email, token.Value);
                await email.SendPasswordResetAsync(user.Value.Email, user.Value.FullName, link, cancellationToken);
                await audit.WriteAsync(new AuthAuditLog(user.Value.Id, AuthEvent.PasswordResetRequested), cancellationToken);
            }
        }

        return Result.Success();
    }
}
