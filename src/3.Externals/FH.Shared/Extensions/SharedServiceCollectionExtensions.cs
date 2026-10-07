using System.Reflection;
using FH.Shared.Application.Behaviors;
using FH.Shared.Domain.Repositories;
using FH.Shared.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Shared.Extensions;

public static class SharedServiceCollectionExtensions
{
    /// <summary>
    /// Registra los servicios base de persistencia para el patrón Repository y Unit of Work.
    /// Requiere que el DbContext (por ejemplo CustomerDbContext) ya esté configurado en IServiceCollection.
    /// </summary>
    public static IServiceCollection AddSharedRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Configura MediatR para soporte de CQRS y Mediator en la solución, registrando handlers
    /// del ensamblado compartido y opcionalmente ensamblados adicionales de módulos.
    /// </summary>
    public static IServiceCollection AddSharedCqrs(
        this IServiceCollection services,
        params Assembly[] additionalAssemblies)
    {
        var assembliesToScan = new List<Assembly>
        {
            typeof(SharedServiceCollectionExtensions).Assembly
        };

        if (additionalAssemblies is { Length: > 0 })
        {
            assembliesToScan.AddRange(additionalAssemblies.Where(a => a is not null));
        }

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assembliesToScan.Distinct().ToArray());
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        return services;
    }
}
