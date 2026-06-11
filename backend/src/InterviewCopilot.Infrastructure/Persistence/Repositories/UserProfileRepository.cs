using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace InterviewCopilot.Infrastructure.Persistence.Repositories;

public sealed class UserProfileRepository(AppDbContext db) : IUserProfileRepository
{
    public async Task<UserProfile?> GetAsync(CandidateId id, CancellationToken ct = default) =>
        await db.UserProfiles.FirstOrDefaultAsync(p => p.Id == id, ct);

    public void Add(UserProfile profile) => db.UserProfiles.Add(profile);
}
