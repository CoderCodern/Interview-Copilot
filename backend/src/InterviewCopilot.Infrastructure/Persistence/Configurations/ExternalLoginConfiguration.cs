using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewCopilot.Infrastructure.Persistence.Configurations;

/// <summary>Maps the ExternalLogin entity to <c>external_logins</c> (Doc 17 §5.2/§6.4).</summary>
public sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("external_logins");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new ExternalLoginId(value))
            .HasColumnName("id");

        builder.Property(e => e.OwnerId)
            .HasConversion(id => id.Value, value => new CandidateId(value))
            .HasColumnName("user_id");

        builder.Property(e => e.Provider).HasColumnName("provider").IsRequired();
        builder.Property(e => e.ProviderKey).HasColumnName("provider_key").IsRequired();
        builder.Property(e => e.Email).HasColumnName("email");
        builder.Property(e => e.DisplayName).HasColumnName("display_name");
        builder.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(e => new { e.Provider, e.ProviderKey })
            .IsUnique()
            .HasDatabaseName("ux_external_logins_provider_key");

        builder.HasIndex(e => e.OwnerId).HasDatabaseName("ix_external_logins_user");

        builder.Ignore(e => e.DomainEvents);
    }
}
