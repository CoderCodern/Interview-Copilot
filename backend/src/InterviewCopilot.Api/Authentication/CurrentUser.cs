using System.Security.Claims;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Api.Authentication;

/// <summary>Resolves the authenticated candidate from the validated JWT <c>sub</c> (Doc 10 §2/§3).</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public CandidateId Id
    {
        get
        {
            var sub = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? Principal?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id)
                ? new CandidateId(id)
                : throw new UnauthorizedAccessException("No authenticated candidate.");
        }
    }
}
