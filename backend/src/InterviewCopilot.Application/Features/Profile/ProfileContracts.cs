using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Profile;

/// <summary>Career fields of the profile (Doc 17 §4).</summary>
public sealed record CareerDto(string? CurrentPosition, int? YearsOfExperience, string? Industry, string? PreferredRole);

/// <summary>Resume status surfaced on the profile.</summary>
public sealed record ResumeStatusDto(bool Uploaded, bool Parsed, DateTimeOffset? LastUpdated);

/// <summary>Preferences (theme/language/notifications).</summary>
public sealed record PreferencesDto(string Theme, string Language, NotificationSettings Notifications);

/// <summary>Onboarding progress for the wizard (Doc 17 §8).</summary>
public sealed record OnboardingDto(bool Profile, bool Resume, bool TargetRole, bool FirstAnalysis, bool IsComplete, DateTimeOffset? CompletedAt);

/// <summary>The full /me payload: identity + profile + onboarding (Doc 17 §6).</summary>
public sealed record MeResponse(
    Guid Id,
    string Email,
    string FullName,
    bool EmailConfirmed,
    string[] Roles,
    string Plan,
    string? AvatarUrl,
    CareerDto Career,
    ResumeStatusDto Resume,
    PreferencesDto Preferences,
    OnboardingDto Onboarding);

public static class ProfileMapping
{
    public static CareerDto ToDto(this CareerProfile c) =>
        new(c.CurrentPosition, c.YearsOfExperience, c.Industry, c.PreferredRole);

    public static ResumeStatusDto ToDto(this ResumeStatus r) =>
        new(r.Uploaded, r.Parsed, r.LastUpdated);

    public static OnboardingDto ToDto(this OnboardingProgress o) =>
        new(o.Profile, o.Resume, o.TargetRole, o.FirstAnalysis, o.IsComplete, o.CompletedAt);
}
