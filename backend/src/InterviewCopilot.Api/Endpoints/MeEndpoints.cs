using InterviewCopilot.Application.Features.Profile.CompleteOnboardingStep;
using InterviewCopilot.Application.Features.Profile.GetMe;
using InterviewCopilot.Application.Features.Profile.UpdateProfile;
using InterviewCopilot.Domain.Users;
using MediatR;

namespace InterviewCopilot.Api.Endpoints;

/// <summary>The /me resource group: current user profile + onboarding (Doc 17 §6/§8). Authorized.</summary>
public static class MeEndpoints
{
    public sealed record UpdateProfileRequest(
        string? CurrentPosition,
        int? YearsOfExperience,
        string? Industry,
        string? PreferredRole,
        string? Theme,
        string? Language,
        NotificationSettings? Notifications);

    public static RouteGroupBuilder MapMeEndpoints(this RouteGroupBuilder group)
    {
        var me = group.MapGroup("/me").WithTags("Profile");

        me.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMeQuery(), ct);
            return result.ToOk();
        }).WithName("GetMe");

        me.MapPut("/profile", async (UpdateProfileRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateProfileCommand(
                req.CurrentPosition, req.YearsOfExperience, req.Industry, req.PreferredRole,
                req.Theme, req.Language, req.Notifications), ct);
            return result.ToOk();
        }).WithName("UpdateProfile");

        me.MapPost("/onboarding/{step}", async (OnboardingStep step, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CompleteOnboardingStepCommand(step), ct);
            return result.ToOk();
        }).WithName("CompleteOnboardingStep");

        return group;
    }
}
