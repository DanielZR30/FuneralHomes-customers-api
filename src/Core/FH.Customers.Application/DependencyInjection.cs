using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using FH.Customers.Application.Events;
using FH.Customers.Application.Mediator;

namespace FH.Customers.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la capa Application: el mediador propio, los casos de uso (handlers de comandos,
    /// consultas y eventos de dominio de todos los módulos, que viven en este ensamblado)
    /// y los validadores de FluentValidation.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IMediator, SimpleMediator>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Casos de uso y handlers de eventos: se registran por reflexión recorriendo el ensamblado
        var handlerInterfaces = new[] { typeof(IRequestHandler<,>), typeof(IDomainEventHandler<>) };

        foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
        {
            foreach (var contract in type.GetInterfaces()
                         .Where(i => i.IsGenericType && handlerInterfaces.Contains(i.GetGenericTypeDefinition())))
            {
                services.AddScoped(contract, type);
            }
        }

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
