namespace InterviewCopilot.Domain.Common;

/// <summary>A record of something that has happened in the domain (past tense).</summary>
public interface IDomainEvent
{
    public Guid EventId => Guid.CreateVersion7();
    public DateTimeOffset OccurredOn => DateTimeOffset.UtcNow;
}
