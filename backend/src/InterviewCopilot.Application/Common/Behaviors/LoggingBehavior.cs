using System.Diagnostics;
using InterviewCopilot.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InterviewCopilot.Application.Common.Behaviors;

/// <summary>Structured start/stop logging with elapsed time and outcome (Doc 01 §4, Doc 11 §3).</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", name);
        var sw = Stopwatch.GetTimestamp();

        var response = await next(ct);

        var elapsedMs = Stopwatch.GetElapsedTime(sw).TotalMilliseconds;
        if (response.IsSuccess)
            logger.LogInformation("Handled {RequestName} in {ElapsedMs:0}ms", name, elapsedMs);
        else
            logger.LogWarning("{RequestName} failed in {ElapsedMs:0}ms: {Code} {Message}",
                name, elapsedMs, response.Error.Code, response.Error.Message);

        return response;
    }
}
