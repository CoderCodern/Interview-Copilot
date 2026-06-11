using InterviewCopilot.Api.Authentication;
using InterviewCopilot.Application.Features.Auth;
using InterviewCopilot.Application.Features.Auth.ChangePassword;
using InterviewCopilot.Application.Features.Auth.ForgotPassword;
using InterviewCopilot.Application.Features.Auth.Login;
using InterviewCopilot.Application.Features.Auth.Logout;
using InterviewCopilot.Application.Features.Auth.LogoutAll;
using InterviewCopilot.Application.Features.Auth.RefreshSession;
using InterviewCopilot.Application.Features.Auth.Register;
using InterviewCopilot.Application.Features.Auth.ResendVerification;
using InterviewCopilot.Application.Features.Auth.ResetPassword;
using InterviewCopilot.Application.Features.Auth.Sessions;
using InterviewCopilot.Application.Features.Auth.VerifyEmail;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Infrastructure.Identity;
using MediatR;
using Microsoft.Extensions.Options;

namespace InterviewCopilot.Api.Endpoints;

/// <summary>The /auth resource group (Doc 17 §6). Public routes are anonymous; session
/// management and password change require authentication.</summary>
public static class AuthEndpoints
{
    // ---- Request bodies (transport DTOs; UA/IP are derived server-side) ----
    public sealed record RegisterRequest(string Email, string Password, string FullName);
    public sealed record VerifyEmailRequest(Guid UserId, string Token);
    public sealed record EmailRequest(string Email);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
    public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    // ---- Response body (refresh token travels in the cookie, not the body, for web) ----
    public sealed record AuthResponseBody(string AccessToken, int ExpiresIn, string TokenType, AuthUserDto User);

    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var auth = group.MapGroup("/auth").WithTags("Auth");

        auth.MapPost("/register", async (RegisterRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RegisterCommand(req.Email, req.Password, req.FullName), ct);
            return result.ToAccepted();
        }).AllowAnonymous().WithName("Register");

        auth.MapPost("/verify-email", async (VerifyEmailRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new VerifyEmailCommand(req.UserId, req.Token), ct);
            return result.ToOk();
        }).AllowAnonymous().WithName("VerifyEmail");

        auth.MapPost("/resend-verification", async (EmailRequest req, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ResendVerificationCommand(req.Email), ct);
            return Results.Accepted();
        }).AllowAnonymous().WithName("ResendVerification");

        auth.MapPost("/login", async (LoginRequest req, HttpContext ctx, ISender sender, RefreshCookie cookie, IOptions<AuthOptions> opt, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new LoginCommand(req.Email, req.Password, UserAgent(ctx), IpHash(ctx, opt.Value)), ct);
            return WriteAuth(ctx, cookie, result);
        }).AllowAnonymous().WithName("Login");

        auth.MapPost("/refresh", async (HttpContext ctx, ISender sender, RefreshCookie cookie, IOptions<AuthOptions> opt, CancellationToken ct) =>
        {
            var raw = cookie.Read(ctx);
            if (string.IsNullOrEmpty(raw))
            {
                return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "auth.refresh_invalid",
                    extensions: new Dictionary<string, object?> { ["code"] = "auth.refresh_invalid" });
            }

            var result = await sender.Send(new RefreshSessionCommand(raw, IpHash(ctx, opt.Value)), ct);
            if (result.IsFailure)
            {
                cookie.Clear(ctx);
            }

            return WriteAuth(ctx, cookie, result);
        }).AllowAnonymous().WithName("Refresh");

        auth.MapPost("/logout", async (HttpContext ctx, ISender sender, RefreshCookie cookie, CancellationToken ct) =>
        {
            var result = await sender.Send(new LogoutCommand(cookie.Read(ctx)), ct);
            cookie.Clear(ctx);
            return result.ToNoContent();
        }).RequireAuthorization().WithName("Logout");

        auth.MapPost("/logout-all", async (HttpContext ctx, ISender sender, RefreshCookie cookie, CancellationToken ct) =>
        {
            var result = await sender.Send(new LogoutAllCommand(), ct);
            cookie.Clear(ctx);
            return result.ToNoContent();
        }).RequireAuthorization().WithName("LogoutAll");

        auth.MapPost("/forgot-password", async (EmailRequest req, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ForgotPasswordCommand(req.Email), ct);
            return Results.Accepted();
        }).AllowAnonymous().WithName("ForgotPassword");

        auth.MapPost("/reset-password", async (ResetPasswordRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ResetPasswordCommand(req.Email, req.Token, req.NewPassword), ct);
            return result.ToNoContent();
        }).AllowAnonymous().WithName("ResetPassword");

        auth.MapPost("/change-password", async (ChangePasswordRequest req, HttpContext ctx, ISender sender, RefreshCookie cookie, CancellationToken ct) =>
        {
            var result = await sender.Send(new ChangePasswordCommand(req.CurrentPassword, req.NewPassword), ct);
            if (result.IsSuccess)
            {
                cookie.Clear(ctx); // all sessions revoked — client must re-authenticate
            }

            return result.ToNoContent();
        }).RequireAuthorization().WithName("ChangePassword");

        auth.MapGet("/sessions", async (HttpContext ctx, ISender sender, RefreshCookie cookie, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListSessionsQuery(cookie.Read(ctx)), ct);
            return result.ToOk();
        }).RequireAuthorization().WithName("ListSessions");

        auth.MapDelete("/sessions/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RevokeSessionCommand(id), ct);
            return result.ToNoContent();
        }).RequireAuthorization().WithName("RevokeSession");

        return group;
    }

    private static IResult WriteAuth(HttpContext ctx, RefreshCookie cookie, Result<AuthResult> result) =>
        result.Map(r =>
        {
            cookie.Write(ctx, r.RefreshToken, r.RefreshExpiresAt);
            return Results.Ok(new AuthResponseBody(r.AccessToken, r.ExpiresIn, AuthResult.TokenType, r.User));
        });

    private static string? UserAgent(HttpContext ctx)
    {
        var ua = ctx.Request.Headers.UserAgent.ToString();
        return string.IsNullOrWhiteSpace(ua) ? null : ua;
    }

    private static string? IpHash(HttpContext ctx, AuthOptions opt) =>
        TokenHashing.HashIp(ctx.Connection.RemoteIpAddress?.ToString(), opt.IpHashSalt);
}
