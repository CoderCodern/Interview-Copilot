namespace InterviewCopilot.Domain.Common;

/// <summary>
/// Aggregate root — the consistency and transaction boundary. Only aggregate roots
/// are loaded/saved by repositories (Doc 03, Doc 04 §1).
/// </summary>
public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id)
    where TId : notnull;
