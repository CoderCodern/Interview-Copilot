using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Auth.Login;

// ---- Command ----
/// <summary>Authenticates email/password and starts a session (Doc 17 §6.2). UserAgent/IpHash
/// are supplied by the endpoint for the session record.</summary>
public sealed record LoginCommand(string Email, string Password, string? UserAgent, string? IpHash)
    : ICommand<AuthResult>;

// ---- Validator ----
public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty();
    }
}

// ---- Handler ----
public sealed class LoginHandler(
    IIdentityService identity,
    ITokenService tokens,
    IRefreshTokenService refresh,
    IAuthAuditWriter audit)
    : ICommandHandler<LoginCommand, AuthResult>
{
    public async Task<Result<AuthResult>> Handle(LoginCommand cmd, CancellationToken cancellationToken)
    {
        var result = await identity.ValidateCredentialsAsync(cmd.Email.Trim(), cmd.Password, cancellationToken);
        if (result.IsFailure)
        {
            await audit.WriteAsync(new AuthAuditLog(null, AuthEvent.LoginFailed, cmd.IpHash, cmd.UserAgent,
                metadata: $"{{\"email\":\"{cmd.Email.Trim()}\"}}"), cancellationToken);
            return result.Error;
        }

        var user = result.Value;
        await identity.RecordLoginAsync(user.Id, cancellationToken);

        var issued = await refresh.IssueAsync(user.Id, cmd.UserAgent, cmd.IpHash, cancellationToken);
        var access = tokens.Issue(user, issued.SessionId);

        await audit.WriteAsync(new AuthAuditLog(user.Id, AuthEvent.LoginSucceeded, cmd.IpHash, cmd.UserAgent), cancellationToken);

        return new AuthResult(access.Value, access.ExpiresInSeconds, AuthUserDto.From(user),
            issued.RawToken, issued.ExpiresAt);
    }
}
