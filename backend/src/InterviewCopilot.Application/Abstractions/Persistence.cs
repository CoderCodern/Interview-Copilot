using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Resumes;

namespace InterviewCopilot.Application.Abstractions;

/// <summary>Commits the current transaction and dispatches domain events via the outbox (Doc 01 §5).</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IResumeRepository
{
    Task<Resume?> GetByIdAsync(ResumeId id, CancellationToken ct = default);
    Task<Resume?> GetCurrentAsync(CandidateId ownerId, CancellationToken ct = default);
    void Add(Resume resume);
}

/// <summary>The authenticated caller, resolved from the validated JWT (Doc 10 §3).</summary>
public interface ICurrentUser
{
    CandidateId Id { get; }
    bool IsAuthenticated { get; }
}

/// <summary>Abstraction over object storage (S3) — presigned URLs + reads (Doc 05 §4).</summary>
public interface IBlobStore
{
    Task<string> CreatePresignedUploadUrlAsync(string key, string contentType, TimeSpan ttl, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(BlobReference reference, CancellationToken ct = default);
}

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
