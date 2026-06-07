using System.Text.Json;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Abstractions.Ai;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Resumes;
using Microsoft.Extensions.Logging;

namespace InterviewCopilot.Application.Features.Resumes.AnalyzeResume;

/// <summary>
/// Worker-facing slice: triggered by the <c>ResumeUploaded</c> event. Extracts text, asks the
/// AI (Economy tier) for a schema-constrained profile, validates it, and completes the aggregate
/// (Doc 01 §5, Doc 07 §2/§5).
/// </summary>
public sealed record AnalyzeResumeCommand(Guid ResumeId) : ICommand, ITransactional;

public sealed class AnalyzeResumeHandler(
    IResumeRepository repository,
    IResumeTextExtractor extractor,
    IChatCompletionService ai,
    ILogger<AnalyzeResumeHandler> logger) : ICommandHandler<AnalyzeResumeCommand>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result> Handle(AnalyzeResumeCommand cmd, CancellationToken ct)
    {
        var resume = await repository.GetByIdAsync(new ResumeId(cmd.ResumeId), ct);
        if (resume is null) return Error.NotFound("Resume");

        var start = resume.StartProcessing();
        if (start.IsFailure) return start; // idempotent: already processed/failed

        try
        {
            var text = await extractor.ExtractAsync(resume.Source, ct);

            var request = new ChatRequest(
                Task: AiTask.ResumeParse,
                Messages:
                [
                    new ChatMessage(ChatRole.System, ResumePrompts.System),
                    new ChatMessage(ChatRole.User, text)
                ],
                ResponseJsonSchema: ResumePrompts.ProfileSchema,
                MinQuality: QualityTier.Economy,
                MaxOutputTokens: 4000);

            var result = await ai.CompleteAsync(request, ct);

            var profile = JsonSerializer.Deserialize<ResumeProfile>(result.Content, JsonOptions)
                ?? throw new JsonException("Empty profile.");

            return resume.Complete(profile);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Resume {ResumeId} parse produced invalid output", cmd.ResumeId);
            return resume.Fail("AI returned an unparseable profile.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resume {ResumeId} parsing failed", cmd.ResumeId);
            return resume.Fail(ex.Message);
        }
    }
}

/// <summary>Port for turning a source (file/text) into clean plain text (impl in Infrastructure/Ingestion).</summary>
public interface IResumeTextExtractor
{
    Task<string> ExtractAsync(AnalysisSource source, CancellationToken ct = default);
}
