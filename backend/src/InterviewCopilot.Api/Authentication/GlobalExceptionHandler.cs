using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InterviewCopilot.Api.Authentication;

/// <summary>
/// Translates unhandled exceptions into RFC 9457 ProblemDetails, hiding internals while
/// returning a correlation id for support (Doc 05 §5, Doc 10 §6).
/// </summary>
public sealed partial class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = httpContext.TraceIdentifier;
        LogUnhandledException(logger, correlationId, exception);

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

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception {CorrelationId}")]
    private static partial void LogUnhandledException(ILogger logger, string correlationId, Exception ex);
}
