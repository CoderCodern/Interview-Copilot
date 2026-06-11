using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Auth.Register;

// ---- Response ----
public sealed record RegisterResponse(Guid UserId, string Email, bool EmailConfirmed, string Message);

// ---- Command ----
/// <summary>Creates an email/password account and sends a verification email (Doc 17 §6.1).</summary>
public sealed record RegisterCommand(string Email, string Password, string FullName) : ICommand<RegisterResponse>;

// ---- Validator ----
public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(PasswordPolicy.MinLength).WithMessage(PasswordPolicy.Description);
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(120);
    }
}

// ---- Handler ----
public sealed class RegisterHandler(
    IIdentityService identity,
    IEmailSender email,
    IAuthLinkBuilder links,
    IAuthAuditWriter audit)
    : ICommandHandler<RegisterCommand, RegisterResponse>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand cmd, CancellationToken cancellationToken)
    {
        var created = await identity.RegisterAsync(cmd.Email.Trim(), cmd.Password, cmd.FullName.Trim(), cancellationToken);
        if (created.IsFailure)
        {
            return created.Error;
        }

        var userId = created.Value;
        var token = await identity.GenerateEmailConfirmationTokenAsync(userId, cancellationToken);
        if (token.IsSuccess)
        {
            var link = links.EmailVerificationLink(userId, token.Value);
            await email.SendEmailVerificationAsync(cmd.Email.Trim(), cmd.FullName.Trim(), link, cancellationToken);
        }

        await audit.WriteAsync(new AuthAuditLog(userId, AuthEvent.Registered), cancellationToken);

        return new RegisterResponse(userId.Value, cmd.Email.Trim(), false,
            "Account created. Check your email to verify your account.");
    }
}
