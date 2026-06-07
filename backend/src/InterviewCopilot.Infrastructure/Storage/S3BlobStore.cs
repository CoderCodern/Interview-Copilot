using Amazon.S3;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Infrastructure.Storage;

/// <summary>S3-backed object store: presigned PUT for uploads, read for the worker (Doc 05 §4, Doc 10 §5).</summary>
public sealed class S3BlobStore(/* IAmazonS3 s3, IOptions<StorageOptions> options */) : IBlobStore
{
    public Task<string> CreatePresignedUploadUrlAsync(string key, string contentType, TimeSpan ttl, CancellationToken ct = default) =>
        // TODO: s3.GetPreSignedURL(new GetPreSignedUrlRequest { BucketName=..., Key=key, Verb=PUT, Expires=now+ttl })
        throw new NotImplementedException("Wire AWS S3 presign here (scaffold).");

    public Task<Stream> OpenReadAsync(BlobReference reference, CancellationToken ct = default) =>
        throw new NotImplementedException("Wire AWS S3 GetObject here (scaffold).");
}
