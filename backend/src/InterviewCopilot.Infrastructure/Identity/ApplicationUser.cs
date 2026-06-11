using Microsoft.AspNetCore.Identity;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>
/// The ASP.NET Identity user — the authentication store representation (Doc 17 §1, ADR 0005).
/// Its <c>Id</c> (UUID v7) equals the domain <c>CandidateId</c>/<c>owner_id</c> used everywhere.
/// Profile/career/preferences live on the domain <c>UserProfile</c> aggregate, not here.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Plan { get; set; } = "free";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }
}

/// <summary>The ASP.NET Identity role (Doc 17 §7).</summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string name) : base(name) { }
}
