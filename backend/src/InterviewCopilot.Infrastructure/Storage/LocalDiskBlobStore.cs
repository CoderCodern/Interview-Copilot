using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Infrastructure.Storage;

/// <summary>
/// Development-only IBlobStore: persists blobs to %TEMP%/interview-copilot-blobs.
/// Registered by Program.cs when ASPNETCORE_ENVIRONMENT=Development.
/// </summary>
public sealed class LocalDiskBlobStore : IBlobStore
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "interview-copilot-blobs");

    public Task<string> CreatePresignedUploadUrlAsync(
        string key, string contentType, TimeSpan ttl, CancellationToken ct = default)
    {
        var path = FullPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        // Returns the absolute path; the caller can write the file directly in dev.
        return Task.FromResult(path);
    }

    public Task<Stream> OpenReadAsync(BlobReference reference, CancellationToken ct = default)
    {
        var path = FullPath(reference.Key);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Dev blob not found at {path}", path);
        }

        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }

    private string FullPath(string key) =>
        Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar));
}
