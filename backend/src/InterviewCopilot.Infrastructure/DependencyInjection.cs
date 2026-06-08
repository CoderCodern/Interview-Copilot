using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Abstractions.Ai;
using InterviewCopilot.Application.Features.Resumes.AnalyzeResume;
using InterviewCopilot.Infrastructure.Ai;
using InterviewCopilot.Infrastructure.Ai.Providers;
using InterviewCopilot.Infrastructure.Ingestion;
using InterviewCopilot.Infrastructure.Persistence;
using InterviewCopilot.Infrastructure.Persistence.Repositories;
using InterviewCopilot.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewCopilot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // --- Persistence (Doc 04) ---
        services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(config.GetConnectionString("Postgres"), npg => npg.UseVector()));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IResumeRepository, ResumeRepository>();

        // --- AI subsystem (Doc 07) ---
        services.Configure<AiCatalogOptions>(config.GetSection(AiCatalogOptions.SectionName));
        services.AddSingleton<IChatProvider, OpenAiChatProvider>();
        services.AddSingleton<IChatProvider, ClaudeChatProvider>();
        services.AddSingleton<IChatProvider, GeminiChatProvider>();
        services.AddScoped<IChatCompletionService, AiModelRouter>();
        services.AddScoped<IAiUsageRecorder, AiUsageRecorder>();
        services.AddScoped<IEmbeddingService, EmbeddingService>();

        // --- Ingestion + storage (Doc 05 §4) ---
        services.AddScoped<IResumeTextExtractor, ResumeTextExtractor>();
        services.AddScoped<IBlobStore, S3BlobStore>();

        // HTTP resilience (retries/timeouts/circuit breakers) applies to provider clients (Doc 07 §3).
        return services;
    }
}
