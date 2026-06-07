using System.Runtime.CompilerServices;
using InterviewCopilot.Application.Abstractions.Ai;

namespace InterviewCopilot.Infrastructure.Ai.Providers;

/// <summary>
/// Adapter over the OpenAI SDK. Maps the vendor-neutral <see cref="ChatRequest"/> to the SDK call,
/// requests JSON-schema-constrained output, and reports real token usage back to the router.
/// (Scaffold: SDK wiring marked TODO; shape is production-ready.)
/// </summary>
public sealed class OpenAiChatProvider /* (OpenAIClient client) */ : IChatProvider
{
    public AiProvider Provider => AiProvider.OpenAi;

    public Task<ProviderCompletion> CompleteAsync(string model, ChatRequest request, CancellationToken ct)
    {
        // TODO: var chat = client.GetChatClient(model);
        //       set ResponseFormat = json_schema(request.ResponseJsonSchema), enable prompt caching,
        //       map request.Messages, call CompleteChatAsync, read Usage (incl. cached tokens).
        throw new NotImplementedException("Wire OpenAI SDK here (scaffold).");
    }

    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        string model, ChatRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        // TODO: stream via client.GetChatClient(model).CompleteChatStreamingAsync(...)
        await Task.CompletedTask;
        yield break;
    }
}
