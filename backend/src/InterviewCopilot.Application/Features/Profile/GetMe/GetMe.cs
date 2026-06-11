using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Profile.GetMe;

// ---- Query ----
/// <summary>Returns the current user with profile + onboarding (Doc 17 §6).</summary>
public sealed record GetMeQuery : IQuery<MeResponse>;

// ---- Handler ----
public sealed class GetMeHandler(
    ICurrentUser currentUser,
    IIdentityService identity,
    IUserProfileRepository profiles)
    : IQueryHandler<GetMeQuery, MeResponse>
{
    public async Task<Result<MeResponse>> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        var user = await identity.FindByIdAsync(currentUser.Id, cancellationToken);
        if (user.IsFailure)
        {
            return user.Error;
        }

        // Provision lazily if a verified user somehow has no profile yet.
        var profile = await profiles.GetAsync(currentUser.Id, cancellationToken);
        if (profile is null)
        {
            profile = UserProfile.Create(currentUser.Id);
            profiles.Add(profile);
        }

        var u = user.Value;
        return new MeResponse(
            u.Id.Value, u.Email, u.FullName, u.EmailConfirmed, [.. u.Roles], u.Plan, u.AvatarUrl,
            profile.Career.ToDto(),
            profile.Resume.ToDto(),
            new PreferencesDto(profile.Theme.ToString(), profile.Language, profile.Notifications),
            profile.Onboarding.ToDto());
    }
}
