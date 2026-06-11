namespace InterviewCopilot.Domain.Users;

/// <summary>UI theme preference (Doc 17 §4).</summary>
public enum Theme
{
    System = 0,
    Light = 1,
    Dark = 2
}

/// <summary>
/// Per-user notification toggles, persisted as a jsonb column (Doc 17 §5.2).
/// Immutable record so EF can round-trip it via System.Text.Json.
/// </summary>
public sealed record NotificationSettings(
    bool ProductUpdates = true,
    bool InterviewReminders = true,
    bool WeeklyDigest = true,
    bool MarketingEmails = false)
{
    public static NotificationSettings Default => new();
}
