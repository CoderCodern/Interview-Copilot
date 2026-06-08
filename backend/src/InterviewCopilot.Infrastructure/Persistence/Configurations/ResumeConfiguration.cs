using System.Text.Json;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Resumes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewCopilot.Infrastructure.Persistence.Configurations;

/// <summary>Maps the Resume aggregate to the schema in Doc 04 §3.</summary>
public sealed class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("resumes");
        builder.HasKey(r => r.Id);

        // Strongly-typed IDs ↔ Guid columns.
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new ResumeId(value))
            .HasColumnName("id");

        builder.Property(r => r.OwnerId)
            .HasConversion(id => id.Value, value => new CandidateId(value))
            .HasColumnName("owner_id");

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasColumnName("status");

        builder.Property(r => r.IsCurrent).HasColumnName("is_current");
        builder.Property(r => r.Error).HasColumnName("error");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        // AnalysisSource as an owned JSON column.
        builder.OwnsOne(r => r.Source, s =>
        {
            s.ToJson("source");
            s.OwnsOne(x => x.Blob);
        });

        // ResumeProfile is a complex positional record with nested IReadOnlyList<T> collections.
        // A direct jsonb conversion is simpler and more reliable than OwnsMany for this shape.
        builder.Property(r => r.Profile)
            .HasColumnName("profile")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<ResumeProfile>(v, (JsonSerializerOptions?)null));

        // One current resume per candidate (Doc 04 §3).
        builder.HasIndex(r => r.OwnerId)
            .HasFilter("is_current")
            .IsUnique()
            .HasDatabaseName("ux_resumes_one_current");

        builder.HasIndex(r => new { r.OwnerId, r.CreatedAt });

        builder.Ignore(r => r.DomainEvents);
    }
}
