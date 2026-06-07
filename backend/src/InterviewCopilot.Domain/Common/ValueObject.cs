namespace InterviewCopilot.Domain.Common;

/// <summary>Value object: structural equality over its components, immutable.</summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) =>
        obj is ValueObject other && GetType() == other.GetType()
        && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, (hash, c) => HashCode.Combine(hash, c));
}
