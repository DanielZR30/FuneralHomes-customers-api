using FH.Modules.Beneficiaries.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FH.Modules.Beneficiaries.Domain.Repositories;
using FH.Modules.Beneficiaries.Infrastructure.Repositories;

namespace FH.Modules.Beneficiaries.Extensions;

public static class BeneficiariesModuleExtensions
{
    public static IServiceCollection AddBeneficiariesModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        return services;
    }

    public static IEndpointRouteBuilder MapBeneficiariesModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapBeneficiaryEndpoints();
        return endpoints;
    }
}
