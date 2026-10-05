using FH.Modules.Customer.Domain.Repositories;
using FH.Modules.Customer.Infrastructure.Endpoints;
using FH.Modules.Customer.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.Customer.Extensions;

public static class CustomerModuleExtensions
{
    public static IServiceCollection AddCustomerModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Los handlers de comandos y consultas se descubren por MediatR: el ensamblado
        // de este módulo se agrega en AddSharedCqrs (Program.cs).
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }

    public static IEndpointRouteBuilder MapCustomerModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCustomerEndpoints();
        return endpoints;
    }
}
