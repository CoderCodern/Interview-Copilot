using System.Runtime.CompilerServices;
using InterviewCopilot.Application.Abstractions.Ai;

namespace InterviewCopilot.Infrastructure.Ai.Providers;

/// <summary>Adapter over the Google Gemini SDK (responseSchema + context caching).</summary>
public sealed class GeminiChatProvider : IChatProvider
{
    public AiProvider Provider => AiProvider.Gemini;

    public Task<ProviderCompletion> CompleteAsync(string model, ChatRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException("Wire Mscc.GenerativeAI SDK — use responseSchema for structured JSON output.");

    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        string model, ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
}
