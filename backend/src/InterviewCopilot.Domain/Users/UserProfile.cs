using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Domain.Users;

/// <summary>
/// The candidate's locally-owned profile (Doc 17 §4). 1:1 with the Identity user; its
/// <see cref="Common.Entity{TId}.Id"/> equals <c>ApplicationUser.Id</c> and is the
/// <c>owner_id</c> carried by every owned aggregate (ADR 0005). Authentication facts
/// (email, password hash, lockout) live on the Identity store; this aggregate owns the
/// career/resume/preferences/onboarding state.
/// </summary>
public sealed class UserProfile : AggregateRoot<CandidateId>
{
    private UserProfile(CandidateId id) : base(id)
    {
        Career = CareerProfile.Empty;
        Resume = ResumeStatus.None;
        Notifications = NotificationSettings.Default;
        Onboarding = OnboardingProgress.NotStarted;
        Language = "en";
        CreatedAt = DateTimeOffset.UtcNow;
    }

#pragma warning disable S1144 // EF Core materialization constructor (reflection).
    private UserProfile() : base(default)
    {
        Career = CareerProfile.Empty;
        Resume = ResumeStatus.None;
        Notifications = NotificationSettings.Default;
        Onboarding = OnboardingProgress.NotStarted;
        Language = "en";
    }
#pragma warning restore S1144

    public CareerProfile Career { get; private set; }
    public ResumeStatus Resume { get; private set; }
    public Theme Theme { get; private set; } = Theme.System;
    public string Language { get; private set; }
    public NotificationSettings Notifications { get; private set; }
    public OnboardingProgress Onboarding { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>Creates the empty profile for a freshly verified user (called on EmailVerified).</summary>
    public static UserProfile Create(CandidateId userId) => new(userId);

    public void UpdateCareer(CareerProfile career)
    {
        Career = career;
        Touch();
    }

    public void UpdatePreferences(Theme theme, string language, NotificationSettings notifications)
    {
        Theme = theme;
        Language = string.IsNullOrWhiteSpace(language) ? Language : language;
        Notifications = notifications;
        Touch();
    }

    public void SetResumeUploaded(DateTimeOffset at)
    {
        Resume = Resume with { Uploaded = true, LastUpdated = at };
        Touch();
    }

    public void SetResumeParsed(DateTimeOffset at)
    {
        Resume = Resume with { Uploaded = true, Parsed = true, LastUpdated = at };
        Touch();
    }

    /// <summary>Marks an onboarding step complete (idempotent), raising step/complete events.</summary>
    public Result CompleteOnboardingStep(OnboardingStep step)
    {
        var wasComplete = Onboarding.IsComplete;
        Onboarding = Onboarding.With(step, DateTimeOffset.UtcNow);
        Touch();

        Raise(new OnboardingStepCompleted(Id, step));
        if (!wasComplete && Onboarding.IsComplete)
        {
            Raise(new OnboardingCompleted(Id));
        }

        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

}
