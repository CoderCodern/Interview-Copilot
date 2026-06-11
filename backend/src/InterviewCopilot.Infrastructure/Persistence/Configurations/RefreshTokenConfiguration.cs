using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewCopilot.Infrastructure.Persistence.Configurations;

/// <summary>Maps the RefreshToken child entity to <c>refresh_tokens</c> (Doc 17 §5.2).</summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new RefreshTokenId(value))
            .HasColumnName("id");

        builder.Property(t => t.SessionId)
            .HasConversion(id => id.Value, value => new UserSessionId(value))
            .HasColumnName("session_id");

        builder.Property(t => t.OwnerId)
            .HasConversion(id => id.Value, value => new CandidateId(value))
            .HasColumnName("user_id");

        builder.Property(t => t.TokenHash).HasColumnName("token_hash").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.ExpiresAt).HasColumnName("expires_at");
        builder.Property(t => t.RevokedAt).HasColumnName("revoked_at");
        builder.Property(t => t.CreatedIpHash).HasColumnName("created_ip_hash");

        builder.Property(t => t.ReplacedById)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? (RefreshTokenId?)null : new RefreshTokenId(value.Value))
            .HasColumnName("replaced_by_id");

        builder.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("ix_refresh_tokens_lookup");
        builder.HasIndex(t => t.OwnerId).HasDatabaseName("ix_refresh_tokens_user");

        builder.Ignore(t => t.DomainEvents);
    }
}
