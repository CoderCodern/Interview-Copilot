using FluentAssertions;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Xunit;

namespace InterviewCopilot.Domain.UnitTests.Users;

public class UserProfileTests
{
    [Fact]
    public void Create_starts_with_defaults_and_no_onboarding()
    {
        var profile = UserProfile.Create(CandidateId.New());

        profile.Theme.Should().Be(Theme.System);
        profile.Language.Should().Be("en");
        profile.Onboarding.IsComplete.Should().BeFalse();
        profile.Notifications.Should().Be(NotificationSettings.Default);
    }

    [Fact]
    public void CompleteOnboardingStep_raises_step_event_and_is_idempotent()
    {
        var profile = UserProfile.Create(CandidateId.New());

        profile.CompleteOnboardingStep(OnboardingStep.Profile);
        profile.CompleteOnboardingStep(OnboardingStep.Profile);

        profile.Onboarding.Profile.Should().BeTrue();
        profile.DomainEvents.Should().ContainItemsAssignableTo<OnboardingStepCompleted>();
    }

    [Fact]
    public void Completing_all_steps_marks_complete_and_raises_completed_once()
    {
        var profile = UserProfile.Create(CandidateId.New());

        profile.CompleteOnboardingStep(OnboardingStep.Profile);
        profile.CompleteOnboardingStep(OnboardingStep.Resume);
        profile.CompleteOnboardingStep(OnboardingStep.TargetRole);
        profile.CompleteOnboardingStep(OnboardingStep.FirstAnalysis);

        profile.Onboarding.IsComplete.Should().BeTrue();
        profile.Onboarding.CompletedAt.Should().NotBeNull();
        profile.DomainEvents.OfType<OnboardingCompleted>().Should().ContainSingle();
    }

    [Fact]
    public void UpdateCareer_and_preferences_persist_values()
    {
        var profile = UserProfile.Create(CandidateId.New());

        profile.UpdateCareer(new CareerProfile("Engineer", 6, "Software", "Senior Engineer"));
        profile.UpdatePreferences(Theme.Dark, "fr", new NotificationSettings(MarketingEmails: true));

        profile.Career.IsComplete.Should().BeTrue();
        profile.Theme.Should().Be(Theme.Dark);
        profile.Language.Should().Be("fr");
        profile.Notifications.MarketingEmails.Should().BeTrue();
    }

    [Fact]
    public void SetResumeParsed_implies_uploaded()
    {
        var profile = UserProfile.Create(CandidateId.New());
        var at = DateTimeOffset.UtcNow;

        profile.SetResumeParsed(at);

        profile.Resume.Uploaded.Should().BeTrue();
        profile.Resume.Parsed.Should().BeTrue();
        profile.Resume.LastUpdated.Should().Be(at);
    }
}
