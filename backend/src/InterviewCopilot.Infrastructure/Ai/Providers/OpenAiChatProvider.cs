using System.Runtime.CompilerServices;
using InterviewCopilot.Application.Abstractions.Ai;

namespace InterviewCopilot.Infrastructure.Ai.Providers;

/// <summary>
/// Adapter over the OpenAI SDK. Maps the vendor-neutral <see cref="ChatRequest"/> to the SDK call,
/// requests JSON-schema-constrained output, and reports real token usage back to the router.
/// (Scaffold: SDK wiring not yet implemented; shape is production-ready.)
/// </summary>
public sealed class OpenAiChatProvider : IChatProvider
{
    public AiProvider Provider => AiProvider.OpenAi;

    public Task<ProviderCompletion> CompleteAsync(string model, ChatRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException("Wire OpenAI SDK — set ResponseFormat=json_schema, map messages, call CompleteChatAsync, read Usage.");

    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        string model, ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }
}
