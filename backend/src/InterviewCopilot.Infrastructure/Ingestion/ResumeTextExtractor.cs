using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Features.Resumes.AnalyzeResume;
using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Infrastructure.Ingestion;

/// <summary>
/// Turns a resume source into clean plain text. PDF/DOCX are parsed locally; images are routed
/// to a vision-capable model upstream; text/url are passed through (Doc 02 dynamic, Doc 07).
/// </summary>
public sealed class ResumeTextExtractor(IBlobStore blobStore) : IResumeTextExtractor
{
    public async Task<string> ExtractAsync(AnalysisSource source, CancellationToken ct = default)
    {
        switch (source.Type)
        {
            case SourceType.Text:
                return source.RawText ?? string.Empty;

            case SourceType.Pdf:
            case SourceType.Docx:
            case SourceType.Image:
                await using (var stream = await blobStore.OpenReadAsync(source.Blob!, ct))
                {
                    // TODO: PDF/DOCX text extraction; image OCR or vision-model passthrough.
                    return await ReadPlaceholderAsync(stream, ct);
                }

            default:
                throw new NotSupportedException($"Unsupported resume source: {source.Type}");
        }
    }

    private static Task<string> ReadPlaceholderAsync(Stream _, CancellationToken __) =>
        Task.FromResult(string.Empty); // scaffold
}
