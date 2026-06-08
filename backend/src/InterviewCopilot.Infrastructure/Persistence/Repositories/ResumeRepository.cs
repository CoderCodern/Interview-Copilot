using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Resumes;
using Microsoft.EntityFrameworkCore;

namespace InterviewCopilot.Infrastructure.Persistence.Repositories;

public sealed class ResumeRepository(AppDbContext db) : IResumeRepository
{
    public async Task<Resume?> GetByIdAsync(ResumeId id, CancellationToken ct = default) =>
        await db.Resumes.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Resume?> GetCurrentAsync(CandidateId ownerId, CancellationToken ct = default) =>
        await db.Resumes.FirstOrDefaultAsync(r => r.OwnerId == ownerId && r.IsCurrent, ct);

    public void Add(Resume aggregate) => db.Resumes.Add(aggregate);
}
