using System.Runtime.CompilerServices;
using InterviewCopilot.Application.Abstractions.Ai;

namespace InterviewCopilot.Infrastructure.Ai.Providers;

/// <summary>Adapter over the Anthropic Claude SDK (tool-use / structured output + prompt caching).</summary>
public sealed class ClaudeChatProvider : IChatProvider
{
    public AiProvider Provider => AiProvider.Claude;

    public Task<ProviderCompletion> CompleteAsync(string model, ChatRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException("Wire Anthropic SDK — use cache_control on the stable system-prompt prefix.");

    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        string model, ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
}
