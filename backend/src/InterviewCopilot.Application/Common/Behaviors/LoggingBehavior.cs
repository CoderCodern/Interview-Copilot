using System.Diagnostics;
using InterviewCopilot.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InterviewCopilot.Application.Common.Behaviors;

/// <summary>Structured start/stop logging with elapsed time and outcome (Doc 01 §4, Doc 11 §3).</summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        LogHandling(logger, name);
        var sw = Stopwatch.GetTimestamp();

        var response = await next();

        var elapsedMs = Stopwatch.GetElapsedTime(sw).TotalMilliseconds;
        if (response.IsSuccess)
        {
            LogHandled(logger, name, elapsedMs);
        }
        else
        {
            LogFailed(logger, name, elapsedMs, response.Error.Code, response.Error.Message);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    private static partial void LogHandling(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs}ms")]
    private static partial void LogHandled(ILogger logger, string requestName, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{RequestName} failed in {ElapsedMs}ms: {Code} {Message}")]
    private static partial void LogFailed(ILogger logger, string requestName, double elapsedMs, string code, string message);
}
