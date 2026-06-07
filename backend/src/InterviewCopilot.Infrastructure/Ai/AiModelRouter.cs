using System.Runtime.CompilerServices;
using InterviewCopilot.Application.Abstractions.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterviewCopilot.Infrastructure.Ai;

/// <summary>
/// The provider-agnostic entry point used by the whole application. It:
///  1. resolves the quality tier for the task,
///  2. orders qualifying models cheapest-first,
///  3. calls providers with cross-provider fallback on failure,
///  4. meters token usage and cost.
/// This is where "AI cost optimization" lives (Doc 07 §2/§3/§7).
/// </summary>
public sealed class AiModelRouter(
    IEnumerable<IChatProvider> providers,
    IOptions<AiCatalogOptions> options,
    IAiUsageRecorder usageRecorder,
    ILogger<AiModelRouter> logger) : IChatCompletionService
{
    private readonly AiCatalogOptions _catalog = options.Value;
    private readonly Dictionary<AiProvider, IChatProvider> _providers = providers.ToDictionary(p => p.Provider);

    public async Task<ChatResult> CompleteAsync(ChatRequest request, CancellationToken ct = default)
    {
        var candidates = SelectModels(request);
        Exception? last = null;

        foreach (var model in candidates)
        {
            if (!_providers.TryGetValue(model.Provider, out var provider)) continue;
            try
            {
                var completion = await provider.CompleteAsync(model.Model, request, ct);
                var cost = CostCalculator.Compute(model, completion.PromptTokens, completion.CompletionTokens, completion.Cached);

                await usageRecorder.RecordAsync(request.Task, model, completion, cost, ct);

                return new ChatResult(
                    completion.Content,
                    model.Provider.ToString(),
                    model.Model,
                    new TokenUsage(completion.PromptTokens, completion.CompletionTokens, completion.Cached),
                    cost);
            }
            catch (Exception ex)
            {
                last = ex;
                logger.LogWarning(ex, "Provider {Provider} model {Model} failed; trying next in tier", model.Provider, model.Model);
            }
        }

        throw new AllProvidersUnavailableException("All providers failed for task " + request.Task, last);
    }

    public IAsyncEnumerable<ChatChunk> StreamAsync(ChatRequest request, CancellationToken ct = default)
    {
        var model = SelectModels(request).First();
        return _providers[model.Provider].StreamAsync(model.Model, request, ct);
    }

    /// <summary>Cheapest-qualifying-first ordering, honoring the task tier and the request floor.</summary>
    private List<ModelEntry> SelectModels(ChatRequest request)
    {
        var taskTier = _catalog.TaskTiers.GetValueOrDefault(request.Task, QualityTier.Standard);
        var tier = (QualityTier)Math.Max((int)taskTier, (int)request.MinQuality);

        return _catalog.Models
            .Where(m => m.Enabled && m.Tier == tier)
            .OrderBy(m => m.InputPerMTok + m.OutputPerMTok)                  // cheapest first
            .ThenByDescending(m => m.Provider == _catalog.DefaultProviderTieBreak)
            .ToList();
    }
}

/// <summary>Persists a <c>token_usage</c> row per call for budgeting + dashboards (Doc 04 §5, Doc 11 §8).</summary>
public interface IAiUsageRecorder
{
    Task RecordAsync(AiTask task, ModelEntry model, ProviderCompletion completion, decimal costUsd, CancellationToken ct);
}

public sealed class AllProvidersUnavailableException(string message, Exception? inner)
    : Exception(message, inner);
