using System.Globalization;
using InterviewCopilot.Api.Authentication;
using InterviewCopilot.Api.Endpoints;
using InterviewCopilot.Application;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Domain.Users;
using InterviewCopilot.Infrastructure;
using InterviewCopilot.Infrastructure.Identity;
using InterviewCopilot.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging: Serilog → OTLP (Doc 11 §3) -----------------------------------
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.OpenTelemetry());

// ---- Application + Infrastructure (Clean Architecture composition root) ------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- Current user from validated JWT (Doc 10 §3, Doc 17 §3) -----------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<RefreshCookie>();

// Dev convenience: local disk blob store instead of S3.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IBlobStore, LocalDiskBlobStore>();
}

// ---- AuthN: first-party JWT bearer in every environment (ADR 0005) ----------
builder.Services.AddSingleton<IConfigureNamedOptions<JwtBearerOptions>, JwtBearerSetup>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

// ---- AuthZ: role + policy (Doc 17 §7) ---------------------------------------
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.EmailVerified, p => p.RequireAuthenticatedUser().RequireClaim("email_verified", "true"))
    .AddPolicy(AuthPolicies.RequireAdmin, p => p.RequireRole(Roles.Admin))
    .AddPolicy(AuthPolicies.RequireModerator, p => p.RequireRole(Roles.Moderator, Roles.Admin))
    .AddPolicy(AuthPolicies.RequirePremium, p => p.RequireRole(Roles.PremiumUser, Roles.Admin));

// ---- ProblemDetails (RFC 9457) for all errors (Doc 05 §5) -------------------
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---- OpenTelemetry traces + metrics (Doc 11 §2/§4) --------------------------
var otlpEndpoint = builder.Configuration["OpenTelemetry:Endpoint"];
var hasOtlp = !string.IsNullOrWhiteSpace(otlpEndpoint);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("interview-copilot-api"))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation()
         .AddHttpClientInstrumentation();
        if (hasOtlp) { t.AddOtlpExporter(); }
    })
    .WithMetrics(m =>
    {
        m.AddAspNetCoreInstrumentation()
         .AddHttpClientInstrumentation();
        if (hasOtlp) { m.AddOtlpExporter(); }
    });

// ---- API surface ------------------------------------------------------------
builder.Services.AddOpenApi();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Ensure roles exist for local development (prod seeds via the migration/one-shot task, Doc 09).
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    await IdentitySeeder.EnsureRolesAsync(roleManager);
}

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// Public JWKS for stateless token validation / key rotation (Doc 17 §3.2).
app.MapGet("/.well-known/jwks.json", (RsaSigningKeyProvider keys) =>
    Results.Content(keys.PublicJwksJson(), "application/json")).AllowAnonymous();

// Versioned endpoint groups (Doc 05 §2).
var v1 = app.MapGroup("/api/v1");

// Auth group manages its own per-route authorization (mostly anonymous).
v1.MapAuthEndpoints();

// Everything else is deny-by-default.
var secured = v1.MapGroup(string.Empty).RequireAuthorization();
secured.MapResumeEndpoints();
secured.MapMeEndpoints();

await app.RunAsync();

public partial class Program { protected Program() { } }
