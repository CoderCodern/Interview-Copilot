using InterviewCopilot.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>Ensures the application roles exist (Doc 17 §7). Idempotent; safe to run on startup.</summary>
public static class IdentitySeeder
{
    public static async Task EnsureRolesAsync(RoleManager<ApplicationRole> roleManager, CancellationToken ct = default)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }
    }
}
