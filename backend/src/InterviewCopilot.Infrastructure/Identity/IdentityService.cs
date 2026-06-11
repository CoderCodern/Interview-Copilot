using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>
/// Result-returning adapter over ASP.NET Identity (Doc 17 §1). Translates Identity outcomes
/// into stable domain <see cref="Error"/> codes; never throws for expected failures.
/// </summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn) : IIdentityService
{
    public async Task<Result<CandidateId>> RegisterAsync(string email, string password, string fullName, CancellationToken ct = default)
    {
        if (await users.FindByEmailAsync(email) is not null)
        {
            return Error.Conflict("auth.email_in_use", "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = false
        };

        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            return MapError(created);
        }

        await users.AddToRoleAsync(user, Roles.User);
        return new CandidateId(user.Id);
    }

    public async Task<Result<AuthUser>> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            // Run a dummy check to keep timing comparable, then return the generic error.
            await signIn.CheckPasswordSignInAsync(new ApplicationUser { UserName = "x", PasswordHash = null }, password, lockoutOnFailure: false);
            return Error.Validation("auth.invalid_credentials", "Invalid email or password.");
        }

        var result = await signIn.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            return Error.Conflict("auth.account_locked", "Account is temporarily locked due to failed attempts.");
        }

        if (!result.Succeeded)
        {
            return Error.Validation("auth.invalid_credentials", "Invalid email or password.");
        }

        return await ToAuthUserAsync(user);
    }

    public async Task<Result<AuthUser>> FindByIdAsync(CandidateId id, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(id.Value.ToString());
        return user is null ? Error.NotFound("User") : await ToAuthUserAsync(user);
    }

    public async Task<Result<AuthUser>> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(email);
        return user is null ? Error.NotFound("User") : await ToAuthUserAsync(user);
    }

    public async Task RecordLoginAsync(CandidateId id, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(id.Value.ToString());
        if (user is not null)
        {
            user.LastLoginAt = DateTimeOffset.UtcNow;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await users.UpdateAsync(user);
        }
    }

    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(CandidateId id, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(id.Value.ToString());
        return user is null ? Error.NotFound("User") : await users.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<Result> ConfirmEmailAsync(CandidateId id, string token, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(id.Value.ToString());
        if (user is null)
        {
            return Error.Validation("auth.verify_invalid", "The verification link is invalid or has expired.");
        }

        var result = await users.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? Result.Success()
            : Error.Validation("auth.verify_invalid", "The verification link is invalid or has expired.");
    }

    public async Task<Result<string>> GeneratePasswordResetTokenAsync(CandidateId id, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(id.Value.ToString());
        return user is null ? Error.NotFound("User") : await users.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(CandidateId id, string token, string newPassword, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(id.Value.ToString());
        if (user is null)
        {
            return Error.Validation("auth.reset_invalid", "The reset link is invalid or has expired.");
        }

        var result = await users.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded ? Result.Success() : MapError(result, "auth.reset_invalid");
    }

    public async Task<Result> ChangePasswordAsync(CandidateId id, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(id.Value.ToString());
        if (user is null)
        {
            return Error.NotFound("User");
        }

        var result = await users.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded ? Result.Success() : MapError(result);
    }

    private async Task<AuthUser> ToAuthUserAsync(ApplicationUser user)
    {
        var roles = await users.GetRolesAsync(user);
        return new AuthUser(
            new CandidateId(user.Id),
            user.Email ?? string.Empty,
            user.FullName,
            user.EmailConfirmed,
            [.. roles],
            user.Plan,
            user.AvatarUrl);
    }

    private static Error MapError(IdentityResult result, string fallbackCode = "validation.failed")
    {
        var first = result.Errors.FirstOrDefault();
        if (first is null)
        {
            return Error.Validation(fallbackCode, "The request could not be completed.");
        }

        // Identity codes like PasswordTooShort, PasswordRequiresDigit, etc.
        if (first.Code.StartsWith("Password", StringComparison.Ordinal))
        {
            return Error.Validation("auth.weak_password", string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        if (first.Code.Contains("Duplicate", StringComparison.Ordinal))
        {
            return Error.Conflict("auth.email_in_use", "An account with this email already exists.");
        }

        return Error.Validation(fallbackCode, first.Description);
    }
}
