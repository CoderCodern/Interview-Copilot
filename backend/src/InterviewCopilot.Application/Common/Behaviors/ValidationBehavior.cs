using FluentValidation;
using InterviewCopilot.Domain.Common;
using MediatR;

namespace InterviewCopilot.Application.Common.Behaviors;

/// <summary>
/// Runs all FluentValidators for a request and short-circuits to a typed failure on error,
/// so handlers never see invalid input (Doc 01 §4).
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any()) return await next(ct);

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0) return await next(ct);

        var error = Error.Validation("validation.failed",
            string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

        // TResponse is Result or Result<T>; both can be created from an Error.
        return (TResponse)(object)Result.Failure(error);
    }
}
