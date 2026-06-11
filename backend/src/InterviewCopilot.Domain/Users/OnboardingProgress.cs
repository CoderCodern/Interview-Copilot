namespace InterviewCopilot.Domain.Users;

/// <summary>The four onboarding steps (Doc 17 §8). Skippable and resumable.</summary>
public enum OnboardingStep
{
    Profile = 0,
    Resume = 1,
    TargetRole = 2,
    FirstAnalysis = 3
}

/// <summary>
/// Onboarding completion state, persisted as a jsonb column (Doc 17 §5.2/§8).
/// Immutable: <see cref="With"/> returns an updated copy with <see cref="CompletedAt"/>
/// stamped once every step is done.
/// </summary>
public sealed record OnboardingProgress(
    bool Profile = false,
    bool Resume = false,
    bool TargetRole = false,
    bool FirstAnalysis = false,
    DateTimeOffset? CompletedAt = null)
{
    public static OnboardingProgress NotStarted => new();

    public bool IsComplete => Profile && Resume && TargetRole && FirstAnalysis;

    /// <summary>Returns a copy with <paramref name="step"/> marked complete, stamping
    /// <see cref="CompletedAt"/> when the last step lands.</summary>
    public OnboardingProgress With(OnboardingStep step, DateTimeOffset now)
    {
        var next = step switch
        {
            OnboardingStep.Profile => this with { Profile = true },
            OnboardingStep.Resume => this with { Resume = true },
            OnboardingStep.TargetRole => this with { TargetRole = true },
            OnboardingStep.FirstAnalysis => this with { FirstAnalysis = true },
            _ => this
        };

        return next is { IsComplete: true, CompletedAt: null }
            ? next with { CompletedAt = now }
            : next;
    }
}
