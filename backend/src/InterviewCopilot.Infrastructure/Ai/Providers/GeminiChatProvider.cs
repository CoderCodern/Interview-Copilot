using System.Runtime.CompilerServices;
using InterviewCopilot.Application.Abstractions.Ai;

namespace InterviewCopilot.Infrastructure.Ai.Providers;

/// <summary>Adapter over the Google Gemini SDK (responseSchema + context caching).</summary>
public sealed class GeminiChatProvider : IChatProvider
{
    public AiProvider Provider => AiProvider.Gemini;

    public Task<ProviderCompletion> CompleteAsync(string model, ChatRequest request, CancellationToken ct) =>
        throw new NotImplementedException("Wire Google.GenerativeAI SDK here (scaffold). Use responseSchema for JSON.");

    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        string model, ChatRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        await Task.CompletedTask;
        yield break;
    }
}
