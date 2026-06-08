using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Api.Authentication;

/// <summary>
/// Development-only ICurrentUser: returns a fixed, well-known candidate ID
/// so EF tenant-scoped query filters resolve correctly without a real JWT.
/// Registered only when ASPNETCORE_ENVIRONMENT=Development (Program.cs).
/// </summary>
public sealed class DevCurrentUser : ICurrentUser
{
    internal const string FixedIdString = "00000000-0000-0000-0000-000000000001";

    private static readonly CandidateId FixedId = new(Guid.Parse(FixedIdString));

    public bool IsAuthenticated => true;

    public CandidateId Id => FixedId;
}
