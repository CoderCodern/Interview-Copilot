using FluentValidation;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;

namespace InterviewCopilot.Application.Features.Profile.UpdateProfile;

// ---- Command ----
/// <summary>Updates career fields and preferences for the current user (Doc 17 §6/§8).</summary>
public sealed record UpdateProfileCommand(
    string? CurrentPosition,
    int? YearsOfExperience,
    string? Industry,
    string? PreferredRole,
    string? Theme,
    string? Language,
    NotificationSettings? Notifications) : ICommand<MeResponse>, ITransactional;

// ---- Validator ----
public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(c => c.YearsOfExperience).InclusiveBetween(0, 70).When(c => c.YearsOfExperience is not null);
        RuleFor(c => c.CurrentPosition).MaximumLength(120);
        RuleFor(c => c.Industry).MaximumLength(120);
        RuleFor(c => c.PreferredRole).MaximumLength(120);
        RuleFor(c => c.Theme)
            .Must(t => Enum.TryParse<Theme>(t, ignoreCase: true, out _))
            .When(c => !string.IsNullOrWhiteSpace(c.Theme))
            .WithMessage("Theme must be System, Light, or Dark.");
        RuleFor(c => c.Language).MaximumLength(10);
    }
}

// ---- Handler ----
public sealed class UpdateProfileHandler(
    ICurrentUser currentUser,
    IIdentityService identity,
    IUserProfileRepository profiles)
    : ICommandHandler<UpdateProfileCommand, MeResponse>
{
    public async Task<Result<MeResponse>> Handle(UpdateProfileCommand cmd, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetAsync(currentUser.Id, cancellationToken);
        if (profile is null)
        {
            profile = UserProfile.Create(currentUser.Id);
            profiles.Add(profile);
        }

        profile.UpdateCareer(new CareerProfile(cmd.CurrentPosition, cmd.YearsOfExperience, cmd.Industry, cmd.PreferredRole));

        var theme = Enum.TryParse<Theme>(cmd.Theme, ignoreCase: true, out var t) ? t : profile.Theme;
        profile.UpdatePreferences(theme, cmd.Language ?? profile.Language, cmd.Notifications ?? profile.Notifications);

        var user = await identity.FindByIdAsync(currentUser.Id, cancellationToken);
        if (user.IsFailure)
        {
            return user.Error;
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
