using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;
using Microsoft.Extensions.Options;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>Builds the frontend links embedded in auth emails (Doc 17 §6). Tokens are
/// URL-escaped; the SPA reads them and posts the decoded value back as JSON.</summary>
public sealed class AuthLinkBuilder(IOptions<AuthOptions> options) : IAuthLinkBuilder
{
    private readonly string _origin = options.Value.WebOrigin.TrimEnd('/');

    public string EmailVerificationLink(CandidateId userId, string token) =>
        $"{_origin}/auth/verify-email?userId={userId.Value}&token={Uri.EscapeDataString(token)}";

    public string PasswordResetLink(string email, string token) =>
        $"{_origin}/auth/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
}
