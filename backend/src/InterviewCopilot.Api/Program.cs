using InterviewCopilot.Api.Authentication;
using InterviewCopilot.Api.Endpoints;
using InterviewCopilot.Application;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging: Serilog → OTLP (Doc 11 §3) -----------------------------------
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry());

// ---- Application + Infrastructure (Clean Architecture composition root) ------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- Current user from validated JWT (Doc 10 §3) ----------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ---- AuthN/Z ----------------------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = true;
    });
builder.Services.AddAuthorization();

// ---- ProblemDetails (RFC 9457) for all errors (Doc 05 §5) -------------------
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---- OpenTelemetry traces + metrics (Doc 11 §2/§4) --------------------------
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("interview-copilot-api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsql()
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

// ---- API surface ------------------------------------------------------------
builder.Services.AddOpenApi();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddHealthChecks();
// .AddNpgSql(...).AddRedis(...) wired per Doc 09 §4

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // app.MapScalarApiReference(); // interactive docs in non-prod
}

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// Versioned endpoint groups, one per bounded context (Doc 05 §2).
var v1 = app.MapGroup("/api/v1").RequireAuthorization();
v1.MapResumeEndpoints();
// v1.MapCompanyEndpoints(); v1.MapJobDescriptionEndpoints();
// v1.MapPreparationEndpoints(); v1.MapMockEndpoints();

app.Run();

public partial class Program; // for WebApplicationFactory integration tests
