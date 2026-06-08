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
    public async Task<string> ExtractAsync(AnalysisSource source, CancellationToken cancellationToken = default)
    {
        switch (source.Type)
        {
            case SourceType.Text:
                return source.RawText ?? string.Empty;

            case SourceType.Pdf:
            case SourceType.Docx:
            case SourceType.Image:
                await using (var stream = await blobStore.OpenReadAsync(source.Blob!, cancellationToken))
                {
                    // Scaffold: add PDF/DOCX text extraction and image OCR/vision-model passthrough here.
                    _ = stream;
                    return string.Empty;
                }

            default:
                throw new NotSupportedException($"Unsupported resume source: {source.Type}");
        }
    }
}
