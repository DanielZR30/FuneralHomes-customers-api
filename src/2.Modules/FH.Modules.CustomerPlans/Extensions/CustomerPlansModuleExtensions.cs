using FH.Modules.CustomerPlans.Application;
using FH.Modules.CustomerPlans.Infrastructure;
using FH.Modules.CustomerPlans.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.CustomerPlans.Extensions;

public static class CustomerPlansModuleExtensions
{
    public static IServiceCollection AddCustomerPlansModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddCustomerPlansApplicationServices();
        services.AddCustomerPlansPersistenceServices(configuration);
        return services;
    }

    public static IEndpointRouteBuilder MapCustomerPlansModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapSubscriptionEndpoints();
        return endpoints;
    }
}
