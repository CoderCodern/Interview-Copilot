using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Application.Features.Auth.ResendVerification;

// ---- Command ----
/// <summary>Re-sends the verification email. Always succeeds regardless of whether the email
/// exists or is already confirmed — no account enumeration (Doc 17 §9).</summary>
public sealed record ResendVerificationCommand(string Email) : ICommand;

// ---- Validator ----
public sealed class ResendVerificationValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationValidator() => RuleFor(c => c.Email).NotEmpty().EmailAddress();
}

// ---- Handler ----
public sealed class ResendVerificationHandler(
    IIdentityService identity,
    IEmailSender email,
    IAuthLinkBuilder links)
    : ICommandHandler<ResendVerificationCommand>
{
    public async Task<Result> Handle(ResendVerificationCommand cmd, CancellationToken cancellationToken)
    {
        var user = await identity.FindByEmailAsync(cmd.Email.Trim(), cancellationToken);
        if (user.IsSuccess && !user.Value.EmailConfirmed)
        {
            var token = await identity.GenerateEmailConfirmationTokenAsync(user.Value.Id, cancellationToken);
            if (token.IsSuccess)
            {
                var link = links.EmailVerificationLink(user.Value.Id, token.Value);
                await email.SendEmailVerificationAsync(user.Value.Email, user.Value.FullName, link, cancellationToken);
            }
        }

        return Result.Success();
    }
}
