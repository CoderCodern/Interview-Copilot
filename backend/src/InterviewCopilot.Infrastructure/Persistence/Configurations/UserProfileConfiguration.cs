using System.Text.Json;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewCopilot.Infrastructure.Persistence.Configurations;

/// <summary>Maps the UserProfile aggregate to <c>user_profiles</c> (Doc 17 §5.2).</summary>
public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new CandidateId(value))
            .HasColumnName("user_id");

        builder.Property(p => p.Theme)
            .HasConversion<string>()
            .HasColumnName("theme");

        builder.Property(p => p.Language).HasColumnName("language").HasMaxLength(10);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        // Career + resume status stored as jsonb (repo precedent: simpler/safer than column splitting
        // for value-object records). Promote to columns later if these need to be queried (Doc 17 §5.2).
        builder.Property(p => p.Career)
            .HasColumnName("career")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<CareerProfile>(v, (JsonSerializerOptions?)null)!);

        builder.Property(p => p.Resume)
            .HasColumnName("resume_status")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<ResumeStatus>(v, (JsonSerializerOptions?)null)!);

        builder.Property(p => p.Notifications)
            .HasColumnName("notification_settings")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<NotificationSettings>(v, (JsonSerializerOptions?)null)!);

        builder.Property(p => p.Onboarding)
            .HasColumnName("onboarding")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<OnboardingProgress>(v, (JsonSerializerOptions?)null)!);

        builder.Ignore(p => p.DomainEvents);
    }
}
