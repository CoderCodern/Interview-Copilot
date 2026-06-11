using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewCopilot.Infrastructure.Persistence.Configurations;

/// <summary>Maps the append-only audit log to <c>auth_audit_logs</c> (Doc 17 §5.2/§9).</summary>
public sealed class AuthAuditLogConfiguration : IEntityTypeConfiguration<AuthAuditLog>
{
    public void Configure(EntityTypeBuilder<AuthAuditLog> builder)
    {
        builder.ToTable("auth_audit_logs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new AuthAuditLogId(value))
            .HasColumnName("id");

        builder.Property(a => a.UserId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? (CandidateId?)null : new CandidateId(value.Value))
            .HasColumnName("user_id");

        builder.Property(a => a.Event)
            .HasConversion<string>()
            .HasColumnName("event");

        builder.Property(a => a.IpHash).HasColumnName("ip_hash");
        builder.Property(a => a.UserAgent).HasColumnName("user_agent");
        builder.Property(a => a.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(a => new { a.UserId, a.CreatedAt }).HasDatabaseName("ix_auth_audit_user_time");
        builder.HasIndex(a => new { a.Event, a.CreatedAt }).HasDatabaseName("ix_auth_audit_event_time");

        builder.Ignore(a => a.DomainEvents);
    }
}
