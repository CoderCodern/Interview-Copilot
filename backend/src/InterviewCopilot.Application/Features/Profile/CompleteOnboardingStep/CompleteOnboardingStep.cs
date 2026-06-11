using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Profile.CompleteOnboardingStep;

// ---- Command ----
/// <summary>Marks one onboarding step complete; resumable and skippable (Doc 17 §8).</summary>
public sealed record CompleteOnboardingStepCommand(OnboardingStep Step) : ICommand<OnboardingDto>, ITransactional;

// ---- Validator ----
public sealed class CompleteOnboardingStepValidator : AbstractValidator<CompleteOnboardingStepCommand>
{
    public CompleteOnboardingStepValidator() => RuleFor(c => c.Step).IsInEnum();
}

// ---- Handler ----
public sealed class CompleteOnboardingStepHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles)
    : ICommandHandler<CompleteOnboardingStepCommand, OnboardingDto>
{
    public async Task<Result<OnboardingDto>> Handle(CompleteOnboardingStepCommand cmd, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetAsync(currentUser.Id, cancellationToken);
        if (profile is null)
        {
            profile = UserProfile.Create(currentUser.Id);
            profiles.Add(profile);
        }

        var result = profile.CompleteOnboardingStep(cmd.Step);
        return result.IsFailure ? result.Error : profile.Onboarding.ToDto();
    }
}
