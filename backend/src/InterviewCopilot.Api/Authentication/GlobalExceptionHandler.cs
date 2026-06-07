using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InterviewCopilot.Api.Authentication;

/// <summary>
/// Translates unhandled exceptions into RFC 9457 ProblemDetails, hiding internals while
/// returning a correlation id for support (Doc 05 §5, Doc 10 §6).
/// </summary>
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var correlationId = httpContext.TraceIdentifier;
        logger.LogError(exception, "Unhandled exception {CorrelationId}", correlationId);

        var (status, code) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "auth.unauthenticated"),
            _ => (StatusCodes.Status500InternalServerError, "internal.error")
        };

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = code,
                Extensions = { ["code"] = code, ["correlationId"] = correlationId }
            }
        });
    }
}
