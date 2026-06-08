using InterviewCopilot.Application.Abstractions.Ai;
using InterviewCopilot.Infrastructure.Persistence;

namespace InterviewCopilot.Infrastructure.Ai;

/// <summary>Writes a <c>token_usage</c> row per AI call for budgeting + cost dashboards (Doc 04 §5, Doc 11 §8).</summary>
public sealed class AiUsageRecorder(AppDbContext db) : IAiUsageRecorder
{
    public Task RecordAsync(AiTask task, ModelEntry model, ProviderCompletion completion, decimal costUsd, CancellationToken cancellationToken)
    {
        // Scaffold: wire db.Add(new TokenUsageRow(...)) here and save within the surrounding unit of work.
        _ = db;
        return Task.CompletedTask;
    }
}

/// <summary>Embedding adapter feeding pgvector; dimension is pinned per model (Doc 04 §4).</summary>
public sealed class EmbeddingService : IEmbeddingService
{
    public int Dimensions => 1536;

    public Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken ct = default) =>
        throw new NotImplementedException("Wire embedding model (e.g. text-embedding-3-small) here.");
}
