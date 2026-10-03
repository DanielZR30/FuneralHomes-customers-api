using FH.Modules.Customer.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.Customer.Extensions;

public static class CustomerModuleExtensions
{
    public static IServiceCollection AddCustomerModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Registro de servicios de aplicación, repositorios y validadores del módulo Customer
        return services;
    }

    public static IEndpointRouteBuilder MapCustomerModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCustomerEndpoints();
        return endpoints;
    }
}
