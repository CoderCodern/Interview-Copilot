namespace InterviewCopilot.Domain.Users;

/// <summary>
/// A denormalized view of the candidate's resume state, surfaced on the profile so the
/// UI/onboarding can react without joining the Resume aggregate (Doc 17 §4/§8).
/// Mapped to columns via an EF complex property.
/// </summary>
public sealed record ResumeStatus(
    bool Uploaded = false,
    bool Parsed = false,
    DateTimeOffset? LastUpdated = null)
{
    public static ResumeStatus None => new();
}
