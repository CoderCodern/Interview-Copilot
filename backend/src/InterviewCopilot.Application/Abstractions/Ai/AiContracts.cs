namespace InterviewCopilot.Application.Abstractions.Ai;

/// <summary>Logical AI tasks. Each maps to a quality tier in routing policy (Doc 07 §2).</summary>
public enum AiTask
{
    ResumeParse,
    JobDescriptionAnalysis,
    CompanyOverview,
    CompanySectionSynthesis,
    GapRecommendation,
    PreparationGeneration,
    MockQuestion,
    MockScoring
}

/// <summary>Quality tier decouples "what this task needs" from "which model is cheapest today".</summary>
public enum QualityTier { Economy, Standard, Premium }

public enum ChatRole { System, User, Assistant }

public sealed record ChatMessage(ChatRole Role, string Content);

/// <summary>
/// Vendor-neutral chat request. The router fills the concrete model from policy; callers
/// never name a provider/model (Doc 07 §1).
/// </summary>
public sealed record ChatRequest(
    AiTask Task,
    IReadOnlyList<ChatMessage> Messages,
    string? ResponseJsonSchema = null,
    QualityTier MinQuality = QualityTier.Standard,
    int? MaxOutputTokens = null,
    bool AllowCache = true);

public sealed record TokenUsage(int PromptTokens, int CompletionTokens, bool Cached)
{
    public int TotalTokens => PromptTokens + CompletionTokens;
}

public sealed record ChatResult(
    string Content,
    string Provider,
    string Model,
    TokenUsage Usage,
    decimal CostUsd);

public sealed record ChatChunk(string Delta, bool IsFinal);

/// <summary>Provider-agnostic chat completion port (Doc 07 §1).</summary>
public interface IChatCompletionService
{
    Task<ChatResult> CompleteAsync(ChatRequest request, CancellationToken ct = default);
    IAsyncEnumerable<ChatChunk> StreamAsync(ChatRequest request, CancellationToken ct = default);
}

/// <summary>Embeddings for RAG over pgvector (Doc 04 §4, Doc 07 §4).</summary>
public interface IEmbeddingService
{
    Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken ct = default);
    int Dimensions { get; }
}
