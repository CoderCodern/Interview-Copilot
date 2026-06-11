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

    /// <summary>202 Accepted with the payload — for async/registration-style commands (Doc 05 §1).</summary>
    public static IResult ToAccepted<T>(this Result<T> result) =>
        result.IsSuccess ? Results.Accepted(value: result.Value) : Problem(result.Error);

    /// <summary>Maps a custom success projection while still funnelling failures to ProblemDetails.</summary>
    public static IResult Map<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : Problem(result.Error);

    private static IResult Problem(Error error)
    {
        var status = error.Code switch
        {
            "resource.not_found" => StatusCodes.Status404NotFound,
            "validation.failed" => StatusCodes.Status400BadRequest,
            "auth.forbidden" => StatusCodes.Status403Forbidden,
            "auth.unauthenticated" => StatusCodes.Status401Unauthorized,
            // --- Auth (Doc 17 §6.5) ---
            "auth.invalid_credentials" or "auth.refresh_invalid" or "auth.refresh_reused"
                => StatusCodes.Status401Unauthorized,
            "auth.email_unverified" => StatusCodes.Status403Forbidden,
            "auth.account_locked" => StatusCodes.Status423Locked,
            "auth.email_in_use" => StatusCodes.Status409Conflict,
            "auth.weak_password" or "auth.verify_invalid" or "auth.reset_invalid"
                => StatusCodes.Status400BadRequest,
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
