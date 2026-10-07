using System.Reflection;
using FH.Shared.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Customer.Application;

public static class ApplicationServicesRegistry
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR para CQRS y pipeline behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServicesRegistry).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // FluentValidation descubriendo todos los validadores del ensamblado de Application
        services.AddValidatorsFromAssembly(typeof(ApplicationServicesRegistry).Assembly);

        return services;
    }
}
