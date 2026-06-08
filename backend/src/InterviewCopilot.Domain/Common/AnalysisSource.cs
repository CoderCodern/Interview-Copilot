namespace InterviewCopilot.Domain.Common;

/// <summary>Pointer to an object in blob storage (S3) — Doc 04, Doc 10 §5.</summary>
public sealed class BlobReference : ValueObject
{
    private BlobReference(string key, string contentType, string? checksum)
    {
        Key = key;
        ContentType = contentType;
        Checksum = checksum;
    }

    public string Key { get; }
    public string ContentType { get; }
    public string? Checksum { get; }

    public static Result<BlobReference> Create(string key, string contentType, string? checksum = null) =>
        string.IsNullOrWhiteSpace(key)
            ? Error.Validation("blob.key_required", "Blob key is required.")
            : new BlobReference(key, contentType, checksum);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Key;
        yield return ContentType;
        yield return Checksum;
    }
}

/// <summary>
/// The raw input to any analysis. Discriminated by <see cref="SourceType"/>; the
/// factory enforces that exactly one payload is present (Doc 03 §5).
/// </summary>
public sealed class AnalysisSource : ValueObject
{
    private AnalysisSource(SourceType type, string? url, BlobReference? blob, string? rawText)
    {
        Type = type;
        Url = url;
        Blob = blob;
        RawText = rawText;
    }

    public SourceType Type { get; }
    public string? Url { get; }
    public BlobReference? Blob { get; }
    public string? RawText { get; }

    public static Result<AnalysisSource> FromUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out _)
            ? new AnalysisSource(SourceType.Url, url, null, null)
            : Error.Validation("source.invalid_url", "A valid absolute URL is required.");

    public static Result<AnalysisSource> FromText(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? Error.Validation("source.empty_text", "Text source cannot be empty.")
            : new AnalysisSource(SourceType.Text, null, null, text);

    public static Result<AnalysisSource> FromBlob(SourceType type, BlobReference blob)
    {
        if (type is not (SourceType.Pdf or SourceType.Docx or SourceType.Image))
        {
            return Error.Validation("source.invalid_blob_type", "Blob sources must be Pdf, Docx, or Image.");
        }

        return new AnalysisSource(type, null, blob, null);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Url;
        yield return Blob;
        yield return RawText;
    }
}
