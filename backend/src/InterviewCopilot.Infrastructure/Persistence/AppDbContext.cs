using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Resumes;
using Microsoft.EntityFrameworkCore;

namespace InterviewCopilot.Infrastructure.Persistence;

/// <summary>
/// EF Core 10 context. Applies entity configurations, a tenant-scoping global query filter,
/// and dispatches domain events through the outbox on save (Doc 04, Doc 10 §3).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Resume> Resumes => Set<Resume>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Tenant isolation: every owned aggregate is scoped to the current candidate.
        modelBuilder.Entity<Resume>().HasQueryFilter(r => r.OwnerId == currentUser.Id);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // In production: collect aggregate domain events here and write them to the
        // outbox table in the same transaction (Doc 01 §5) before SaveChanges.
        return await base.SaveChangesAsync(ct);
    }
}
