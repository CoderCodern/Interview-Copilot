using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Users;
using InterviewCopilot.Infrastructure.Persistence;

namespace InterviewCopilot.Infrastructure.Identity;

/// <summary>Appends security audit rows (Doc 17 §9). Self-persists so audit survives even
/// when the surrounding command fails.</summary>
public sealed class AuthAuditWriter(AppDbContext db) : IAuthAuditWriter
{
    public async Task WriteAsync(AuthAuditLog entry, CancellationToken ct = default)
    {
        db.AuthAuditLogs.Add(entry);
        await db.SaveChangesAsync(ct);
    }
}
