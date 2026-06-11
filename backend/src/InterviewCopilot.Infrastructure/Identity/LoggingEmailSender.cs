using InterviewCopilot.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>
/// Default email sink for local/dev: logs the link instead of sending (Doc 17 §9 follow-up).
/// Replace with an Amazon SES implementation in production (verified domain + SPF/DKIM/DMARC).
/// Never logs in production environments (guarded by registration in DependencyInjection).
/// </summary>
public sealed partial class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendEmailVerificationAsync(string email, string fullName, string verificationLink, CancellationToken ct = default)
    {
        LogVerification(logger, email, verificationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string fullName, string resetLink, CancellationToken ct = default)
    {
        LogReset(logger, email, resetLink);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEV EMAIL] Verify {Email}: {Link}")]
    private static partial void LogVerification(ILogger logger, string email, string link);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEV EMAIL] Reset password {Email}: {Link}")]
    private static partial void LogReset(ILogger logger, string email, string link);
}
