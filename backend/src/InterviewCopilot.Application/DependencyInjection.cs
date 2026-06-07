using System.Reflection;
using FluentValidation;
using InterviewCopilot.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewCopilot.Application;

public static class DependencyInjection
{
    /// <summary>Registers MediatR, validators, and the ordered pipeline behaviors (Doc 01 §4).</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Order matters: logging → validation → unit-of-work.
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
