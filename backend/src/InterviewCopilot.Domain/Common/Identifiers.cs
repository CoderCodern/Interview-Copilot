namespace InterviewCopilot.Domain.Common;

/// <summary>
/// Strongly-typed IDs prevent primitive-obsession bugs (passing a ResumeId where a
/// CandidateId is expected won't compile). Backed by time-ordered UUID v7 (Doc 04 §1).
/// </summary>
public readonly record struct CandidateId(Guid Value)
{
    public static CandidateId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct ResumeId(Guid Value)
{
    public static ResumeId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct CompanyAnalysisId(Guid Value)
{
    public static CompanyAnalysisId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct JobDescriptionId(Guid Value)
{
    public static JobDescriptionId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct PreparationId(Guid Value)
{
    public static PreparationId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}

public readonly record struct MockSessionId(Guid Value)
{
    public static MockSessionId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}
