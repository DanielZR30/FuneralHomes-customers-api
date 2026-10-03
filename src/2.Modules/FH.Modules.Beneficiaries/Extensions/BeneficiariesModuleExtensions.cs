using FH.Modules.Beneficiaries.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.Beneficiaries.Extensions;

public static class BeneficiariesModuleExtensions
{
    public static IServiceCollection AddBeneficiariesModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Registro de servicios de aplicación y repositorios del módulo Beneficiaries
        return services;
    }

    public static IEndpointRouteBuilder MapBeneficiariesModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapBeneficiaryEndpoints();
        return endpoints;
    }
}
