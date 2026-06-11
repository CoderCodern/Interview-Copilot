namespace InterviewCopilot.Domain.Users;

/// <summary>
/// The candidate's career context (Doc 17 §4). Mapped to columns on <c>user_profiles</c>
/// via an EF complex property; all members are optional and filled during onboarding.
/// </summary>
public sealed record CareerProfile(
    string? CurrentPosition = null,
    int? YearsOfExperience = null,
    string? Industry = null,
    string? PreferredRole = null)
{
    public static CareerProfile Empty => new();

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(CurrentPosition)
        && YearsOfExperience is not null
        && !string.IsNullOrWhiteSpace(PreferredRole);
}
