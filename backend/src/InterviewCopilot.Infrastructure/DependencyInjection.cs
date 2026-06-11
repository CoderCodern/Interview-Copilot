using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Abstractions.Ai;
using InterviewCopilot.Application.Features.Resumes.AnalyzeResume;
using InterviewCopilot.Infrastructure.Ai;
using InterviewCopilot.Infrastructure.Ai.Providers;
using InterviewCopilot.Infrastructure.Identity;
using InterviewCopilot.Infrastructure.Ingestion;
using InterviewCopilot.Infrastructure.Persistence;
using InterviewCopilot.Infrastructure.Persistence.Repositories;
using InterviewCopilot.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
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

        // --- Identity & auth (Doc 17, ADR 0005) ---
        services.Configure<AuthOptions>(config.GetSection(AuthOptions.SectionName));
        services.Configure<GoogleOAuthOptions>(config.GetSection(GoogleOAuthOptions.SectionName));

        services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.SignIn.RequireConfirmedEmail = false; // login allowed; gated actions require EmailVerified policy
                o.Password.RequiredLength = 10;
                o.Password.RequireDigit = true;
                o.Password.RequireUppercase = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireNonAlphanumeric = false;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                o.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<RsaSigningKeyProvider>();
        services.AddSingleton<IAuthLinkBuilder, AuthLinkBuilder>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IAuthAuditWriter, AuthAuditWriter>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

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
