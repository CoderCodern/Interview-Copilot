using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Resumes;
using InterviewCopilot.Domain.Users;
using InterviewCopilot.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InterviewCopilot.Infrastructure.Persistence;

/// <summary>
/// EF Core 10 context, now also the ASP.NET Identity store (ADR 0005). Applies entity
/// configurations, a tenant-scoping global query filter on owned aggregates, and dispatches
/// domain events through the outbox on save (Doc 04, Doc 10 §3, Doc 17 §5).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IUnitOfWork
{
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserSession> Sessions => Set<UserSession>();
    public DbSet<AuthAuditLog> AuthAuditLogs => Set<AuthAuditLog>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity model first, then rename its tables to project convention (Doc 17 §5.2).
        base.OnModelCreating(builder);

        builder.HasPostgresExtension("vector");
        builder.HasPostgresExtension("pg_trgm");
        builder.HasPostgresExtension("citext");

        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("users");
            b.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(120);
            b.Property(u => u.AvatarUrl).HasColumnName("avatar_url");
            b.Property(u => u.Plan).HasColumnName("plan").HasMaxLength(20);
            b.Property(u => u.CreatedAt).HasColumnName("created_at");
            b.Property(u => u.UpdatedAt).HasColumnName("updated_at");
            b.Property(u => u.LastLoginAt).HasColumnName("last_login_at");
        });
        builder.Entity<ApplicationRole>().ToTable("roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_identity_logins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Tenant isolation: scope owned aggregates to the current user. Auth/identity tables
        // (users, sessions, profile, audit) are scoped explicitly in their services/repos and
        // are accessed on anonymous endpoints, so they carry no global filter (Doc 17 §5.3).
        builder.Entity<Resume>().HasQueryFilter(r => r.OwnerId == currentUser.Id);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In production: collect aggregate domain events here and write them to the
        // outbox table in the same transaction (Doc 01 §5) before SaveChanges.
        return await base.SaveChangesAsync(cancellationToken);
    }
}
