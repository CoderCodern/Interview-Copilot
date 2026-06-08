using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Resumes;

namespace InterviewCopilot.Application.Features.Resumes.GetResume;

public sealed record GetResumeQuery(Guid ResumeId) : IQuery<ResumeDetail>;

public sealed record ResumeDetail(
    Guid Id,
    string Status,
    bool IsCurrent,
    ResumeProfile? Profile,
    string? Error,
    DateTimeOffset CreatedAt);

public sealed class GetResumeHandler(IResumeRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetResumeQuery, ResumeDetail>
{
    public async Task<Result<ResumeDetail>> Handle(GetResumeQuery query, CancellationToken cancellationToken)
    {
        var resume = await repository.GetByIdAsync(new ResumeId(query.ResumeId), cancellationToken);

        // Ownership check is also enforced by the EF global query filter (defense in depth, Doc 10 §3).
        if (resume is null || resume.OwnerId != currentUser.Id)
        {
            return Error.NotFound("Resume");
        }

        return new ResumeDetail(
            resume.Id.Value,
            resume.Status.ToString(),
            resume.IsCurrent,
            resume.Profile,
            resume.Error,
            resume.CreatedAt);
    }
}
