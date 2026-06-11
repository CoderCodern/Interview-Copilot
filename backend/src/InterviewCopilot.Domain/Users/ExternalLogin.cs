using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Domain.Users;

/// <summary>
/// A linked social identity (Doc 17 §6.4). Keyed by (provider, providerKey); linking is
/// only permitted on a verified email to prevent account hijack.
/// </summary>
public sealed class ExternalLogin : Entity<ExternalLoginId>
{
    public ExternalLogin(
        CandidateId ownerId,
        string provider,
        string providerKey,
        string? email,
        string? displayName,
        string? avatarUrl) : base(ExternalLoginId.New())
    {
        OwnerId = ownerId;
        Provider = provider;
        ProviderKey = providerKey;
        Email = email;
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public CandidateId OwnerId { get; private set; }
    public string Provider { get; private set; }
    public string ProviderKey { get; private set; }
    public string? Email { get; private set; }
    public string? DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public const string Google = "google";

#pragma warning disable S1144 // EF Core materialization constructor.
    private ExternalLogin() : base(default) { Provider = null!; ProviderKey = null!; }
#pragma warning restore S1144
}
