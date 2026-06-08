using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Resumes;

namespace InterviewCopilot.Application.Abstractions;

/// <summary>Commits the current transaction and dispatches domain events via the outbox (Doc 01 §5).</summary>
public interface IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IResumeRepository
{
    public Task<Resume?> GetByIdAsync(ResumeId id, CancellationToken ct = default);
    public Task<Resume?> GetCurrentAsync(CandidateId ownerId, CancellationToken ct = default);
    public void Add(Resume aggregate);
}

/// <summary>The authenticated caller, resolved from the validated JWT (Doc 10 §3).</summary>
public interface ICurrentUser
{
    public CandidateId Id { get; }
    public bool IsAuthenticated { get; }
}

/// <summary>Abstraction over object storage (S3) — presigned URLs + reads (Doc 05 §4).</summary>
public interface IBlobStore
{
    public Task<string> CreatePresignedUploadUrlAsync(string key, string contentType, TimeSpan ttl, CancellationToken ct = default);
    public Task<Stream> OpenReadAsync(BlobReference reference, CancellationToken ct = default);
}

public interface IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; }
}
