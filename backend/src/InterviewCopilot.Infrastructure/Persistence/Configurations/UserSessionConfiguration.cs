using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewCopilot.Infrastructure.Persistence.Configurations;

/// <summary>Maps the UserSession aggregate + its refresh-token chain (Doc 17 §5.2).</summary>
public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new UserSessionId(value))
            .HasColumnName("id");

        builder.Property(s => s.OwnerId)
            .HasConversion(id => id.Value, value => new CandidateId(value))
            .HasColumnName("user_id");

        builder.Property(s => s.UserAgent).HasColumnName("user_agent");
        builder.Property(s => s.IpHash).HasColumnName("ip_hash");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(s => s.RevokedAt).HasColumnName("revoked_at");

        // One-to-many to RefreshToken via the read-only navigation backed by the _tokens field.
        builder.HasMany(s => s.Tokens)
            .WithOne()
            .HasForeignKey(t => t.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        var nav = builder.Metadata.FindNavigation(nameof(UserSession.Tokens))!;
        nav.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(s => s.OwnerId).HasDatabaseName("ix_user_sessions_user");

        builder.Ignore(s => s.DomainEvents);
    }
}
