using InterviewCopilot.Application.Abstractions.Ai;

namespace InterviewCopilot.Infrastructure.Ai;

/// <summary>Low-level per-vendor adapter. The router selects the model and provider; the
/// provider just executes the call against a concrete model (Doc 07 §1).</summary>
public interface IChatProvider
{
    AiProvider Provider { get; }

    Task<ProviderCompletion> CompleteAsync(string model, ChatRequest request, CancellationToken ct);

    IAsyncEnumerable<ChatChunk> StreamAsync(string model, ChatRequest request, CancellationToken ct);
}

public sealed record ProviderCompletion(string Content, int PromptTokens, int CompletionTokens, bool Cached);
