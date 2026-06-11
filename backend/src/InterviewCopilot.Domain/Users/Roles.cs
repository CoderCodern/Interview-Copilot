namespace InterviewCopilot.Domain.Users;

/// <summary>
/// Application roles (Doc 17 §7). <c>User</c> is assigned on registration; the rest are
/// reserved for future expansion. Stored in the Identity role table and emitted as
/// <c>role</c> claims on the access token.
/// </summary>
public static class Roles
{
    public const string User = "User";
    public const string PremiumUser = "PremiumUser";
    public const string Moderator = "Moderator";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [User, PremiumUser, Moderator, Admin];
}

/// <summary>
/// Permission constants (Doc 17 §7). Roles map to permissions so adding a role is a
/// mapping change, not a code fork (CLAUDE.md §2 — YAGNI/SOLID).
/// </summary>
public static class Permissions
{
    public const string CompaniesRead = "companies.read";
    public const string CompaniesWrite = "companies.write";
    public const string ResumesWrite = "resumes.write";
    public const string PreparationsWrite = "preparations.write";
    public const string MockWrite = "mock.write";
    public const string PremiumModels = "ai.premium_models";
    public const string ModerationReview = "moderation.review";
    public const string AdminUsersManage = "admin.users.manage";
    public const string AdminAuditRead = "admin.audit.read";
}

/// <summary>Named authorization policies registered in the API (Doc 17 §7).</summary>
public static class AuthPolicies
{
    public const string EmailVerified = "EmailVerified";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireModerator = "RequireModerator";
    public const string RequirePremium = "RequirePremium";
}
