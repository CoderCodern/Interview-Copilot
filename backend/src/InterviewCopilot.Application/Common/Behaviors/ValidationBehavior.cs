using System.Reflection;
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
    // Cached once per generic instantiation — avoids repeated reflection on hot paths.
    private static readonly MethodInfo s_genericFailure =
        typeof(Result).GetMethods(BindingFlags.Public | BindingFlags.Static)
                      .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var error = Error.Validation("validation.failed",
            string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

        return CreateFailure(error);
    }

    // Result.Failure(error) returns the base Result; casting it to Result<T> (derived) throws.
    // When TResponse is Result<T>, call the generic Result.Failure<T>(error) instead.
    private static TResponse CreateFailure(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        return (TResponse)s_genericFailure
            .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0])
            .Invoke(null, [error])!;
    }
}
