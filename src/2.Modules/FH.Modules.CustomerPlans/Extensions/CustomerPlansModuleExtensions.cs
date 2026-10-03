using FH.Modules.CustomerPlans.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.CustomerPlans.Extensions;

public static class CustomerPlansModuleExtensions
{
    public static IServiceCollection AddCustomerPlansModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Registro de servicios de aplicación y repositorios del módulo CustomerPlans
        return services;
    }

    public static IEndpointRouteBuilder MapCustomerPlansModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapSubscriptionEndpoints();
        return endpoints;
    }
}
