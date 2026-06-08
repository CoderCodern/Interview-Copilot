using System.Globalization;
using InterviewCopilot.Api.Authentication;
using InterviewCopilot.Api.Endpoints;
using InterviewCopilot.Application;
using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Infrastructure;
using InterviewCopilot.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// ---- Current user from validated JWT (Doc 10 §3) ----------------------------
builder.Services.AddHttpContextAccessor();

// ---- AuthN/Z ----------------------------------------------------------------
if (builder.Environment.IsDevelopment())
{
    // Dev bypass: every request is auto-authenticated as a fixed candidate.
    // ICurrentUser is also overridden so EF tenant-scoped filters work.
    builder.Services.AddAuthentication("Dev")
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>("Dev", _ => { });
    builder.Services.AddScoped<ICurrentUser, DevCurrentUser>();

    // Dev blob store: local disk instead of S3.
    builder.Services.AddScoped<IBlobStore, LocalDiskBlobStore>();
}
else
{
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Auth:Authority"];
            options.Audience = builder.Configuration["Auth:Audience"];
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidateAudience = true;
        });
}

builder.Services.AddAuthorization();

// ---- ProblemDetails (RFC 9457) for all errors (Doc 05 §5) -------------------
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---- OpenTelemetry traces + metrics (Doc 11 §2/§4) --------------------------
// OTLP export is skipped when no endpoint is configured (e.g. local dev).
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
    .AllowAnyHeader().AllowAnyMethod()));
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
    // Scalar interactive API explorer → /scalar/v1
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// Versioned endpoint groups, one per bounded context (Doc 05 §2).
var v1 = app.MapGroup("/api/v1").RequireAuthorization();
v1.MapResumeEndpoints();

await app.RunAsync();

public partial class Program { protected Program() { } }
