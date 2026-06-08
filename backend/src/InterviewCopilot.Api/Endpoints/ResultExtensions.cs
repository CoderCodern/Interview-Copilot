using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Api.Endpoints;

/// <summary>Maps a domain <see cref="Result"/> to an HTTP response with ProblemDetails on failure (Doc 05 §5).</summary>
public static class ResultExtensions
{
    public static IResult ToOk<T>(this Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);

    public static IResult ToCreated<T>(this Result<T> result, Func<T, string> location) =>
        result.IsSuccess
            ? Results.Created(location(result.Value), result.Value)
            : Problem(result.Error);

    public static IResult ToNoContent(this Result result) =>
        result.IsSuccess ? Results.NoContent() : Problem(result.Error);

    private static IResult Problem(Error error)
    {
        var status = error.Code switch
        {
            "resource.not_found" => StatusCodes.Status404NotFound,
            "validation.failed" => StatusCodes.Status400BadRequest,
            "auth.forbidden" => StatusCodes.Status403Forbidden,
            "auth.unauthenticated" => StatusCodes.Status401Unauthorized,
            _ when error.Code.EndsWith(".inputs_not_ready", StringComparison.Ordinal) => StatusCodes.Status422UnprocessableEntity,
            _ when error.Code.Contains("already") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            statusCode: status,
            title: error.Code,
            detail: error.Message,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
