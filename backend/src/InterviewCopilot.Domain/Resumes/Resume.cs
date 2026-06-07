using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Domain.Resumes;

/// <summary>
/// Resume aggregate root. Owns the parsed <see cref="ResumeProfile"/> and guards the
/// status lifecycle and the "exactly one current resume per candidate" invariant
/// (Doc 03 §4.2, Doc 04 §3).
/// </summary>
public sealed class Resume : AggregateRoot<ResumeId>
{
    private Resume(ResumeId id, CandidateId ownerId, AnalysisSource source) : base(id)
    {
        OwnerId = ownerId;
        Source = source;
        Status = AnalysisStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // EF Core materialization constructor.
    private Resume(ResumeId id) : base(id) { OwnerId = default; Source = null!; }

    public CandidateId OwnerId { get; }
    public AnalysisSource Source { get; private set; }
    public AnalysisStatus Status { get; private set; }
    public ResumeProfile? Profile { get; private set; }
    public bool IsCurrent { get; private set; }
    public string? Error { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>Factory — creating a resume raises <see cref="ResumeUploaded"/> for the ingestion pipeline.</summary>
    public static Resume Upload(CandidateId ownerId, AnalysisSource source)
    {
        var resume = new Resume(ResumeId.New(), ownerId, source);
        resume.Raise(new ResumeUploaded(resume.Id, ownerId));
        return resume;
    }

    public Result StartProcessing()
    {
        if (Status != AnalysisStatus.Pending)
            return Common.Error.Conflict("resume.invalid_transition", $"Cannot start processing from '{Status}'.");
        Status = AnalysisStatus.Processing;
        Touch();
        return Result.Success();
    }

    public Result Complete(ResumeProfile profile)
    {
        if (Status != AnalysisStatus.Processing)
            return Common.Error.Conflict("resume.invalid_transition", $"Cannot complete from '{Status}'.");
        Profile = profile;
        Status = AnalysisStatus.Completed;
        Touch();
        Raise(new ResumeParsed(Id, OwnerId));
        return Result.Success();
    }

    public Result Fail(string reason)
    {
        if (Status is AnalysisStatus.Completed)
            return Common.Error.Conflict("resume.invalid_transition", "A completed resume cannot fail.");
        Status = AnalysisStatus.Failed;
        Error = reason;
        Touch();
        Raise(new ResumeParseFailed(Id, OwnerId, reason));
        return Result.Success();
    }

    /// <summary>Marks this resume current. The "single current" rule is enforced by a
    /// unique partial index plus the application flipping the previous current off (Doc 04 §3).</summary>
    public void MarkCurrent()
    {
        IsCurrent = true;
        Touch();
    }

    public void Supersede()
    {
        IsCurrent = false;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
