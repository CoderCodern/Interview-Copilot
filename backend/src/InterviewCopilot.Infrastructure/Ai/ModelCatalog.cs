using InterviewCopilot.Application.Abstractions.Ai;

namespace InterviewCopilot.Infrastructure.Ai;

public enum AiProvider { OpenAi, Claude, Gemini }

/// <summary>
/// One routable model with its price and capabilities. Prices are USD per 1M tokens and are
/// configuration, not code, so vendor price changes are a config edit (Doc 07 §2, Doc 14 §1).
/// </summary>
public sealed record ModelEntry(
    AiProvider Provider,
    string Model,
    QualityTier Tier,
    decimal InputPerMTok,
    decimal OutputPerMTok,
    bool SupportsCaching,
    int ContextWindow,
    bool Enabled = true);

/// <summary>Bound from configuration (appsettings / Secrets). Defaults mirror Doc 14 §1 (June 2026).</summary>
public sealed class AiCatalogOptions
{
    public const string SectionName = "Ai:Catalog";

    public List<ModelEntry> Models { get; init; } =
    [
        // Economy
        new(AiProvider.Gemini, "gemini-3.1-flash-lite", QualityTier.Economy, 0.25m, 1.50m, true, 1_000_000),
        new(AiProvider.OpenAi, "gpt-5.4-nano",          QualityTier.Economy, 0.20m, 1.25m, true, 400_000),
        new(AiProvider.Claude, "claude-haiku-4-5",      QualityTier.Economy, 1.00m, 5.00m, true, 200_000),
        // Standard
        new(AiProvider.Gemini, "gemini-3.5-flash",      QualityTier.Standard, 1.50m, 9.00m, true, 1_000_000),
        new(AiProvider.OpenAi, "gpt-5.4",               QualityTier.Standard, 2.50m, 15.00m, true, 400_000),
        new(AiProvider.Claude, "claude-sonnet-4-6",     QualityTier.Standard, 3.00m, 15.00m, true, 1_000_000),
        // Premium
        new(AiProvider.Gemini, "gemini-3.1-pro",        QualityTier.Premium, 2.00m, 12.00m, true, 1_000_000),
        new(AiProvider.Claude, "claude-opus-4-8",       QualityTier.Premium, 5.00m, 25.00m, true, 1_000_000),
        new(AiProvider.OpenAi, "gpt-5.5",               QualityTier.Premium, 5.00m, 30.00m, true, 400_000),
    ];

    /// <summary>Default tier for each task; overridable per environment (Doc 07 §2).</summary>
    public Dictionary<AiTask, QualityTier> TaskTiers { get; init; } = new()
    {
        [AiTask.ResumeParse] = QualityTier.Economy,
        [AiTask.JobDescriptionAnalysis] = QualityTier.Economy,
        [AiTask.CompanyOverview] = QualityTier.Standard,
        [AiTask.CompanySectionSynthesis] = QualityTier.Standard,
        [AiTask.GapRecommendation] = QualityTier.Standard,
        [AiTask.PreparationGeneration] = QualityTier.Standard,
        [AiTask.MockQuestion] = QualityTier.Standard,
        [AiTask.MockScoring] = QualityTier.Standard,
    };

    public AiProvider DefaultProviderTieBreak { get; init; } = AiProvider.Gemini;
}

internal static class CostCalculator
{
    public static decimal Compute(ModelEntry m, int promptTokens, int completionTokens, bool cached)
    {
        var inputRate = cached && m.SupportsCaching ? m.InputPerMTok * 0.10m : m.InputPerMTok; // ~90% off cached
        return promptTokens / 1_000_000m * inputRate
             + completionTokens / 1_000_000m * m.OutputPerMTok;
    }
}
