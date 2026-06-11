using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Auth.VerifyEmail;

// ---- Response ----
public sealed record VerifyEmailResponse(bool EmailConfirmed);

// ---- Command ----
/// <summary>Confirms an email with the token from the verification link, then provisions the
/// empty <see cref="UserProfile"/> for the user (Doc 17 §4/§6). Transactional: the profile is
/// committed by the UnitOfWork behavior.</summary>
public sealed record VerifyEmailCommand(Guid UserId, string Token) : ICommand<VerifyEmailResponse>, ITransactional;

// ---- Validator ----
public sealed class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Token).NotEmpty();
    }
}

// ---- Handler ----
public sealed class VerifyEmailHandler(
    IIdentityService identity,
    IUserProfileRepository profiles,
    IAuthAuditWriter audit)
    : ICommandHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    public async Task<Result<VerifyEmailResponse>> Handle(VerifyEmailCommand cmd, CancellationToken cancellationToken)
    {
        var userId = new CandidateId(cmd.UserId);
        var confirmed = await identity.ConfirmEmailAsync(userId, cmd.Token, cancellationToken);
        if (confirmed.IsFailure)
        {
            return confirmed.Error;
        }

        // Provision the profile exactly once.
        if (await profiles.GetAsync(userId, cancellationToken) is null)
        {
            profiles.Add(UserProfile.Create(userId));
        }

        await audit.WriteAsync(new AuthAuditLog(userId, AuthEvent.EmailVerified), cancellationToken);
        return new VerifyEmailResponse(true);
    }
}
