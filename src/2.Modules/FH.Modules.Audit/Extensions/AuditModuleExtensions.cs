using FH.Modules.Audit.Domain.Repositories;
using FH.Modules.Audit.Infrastructure.Endpoints;
using FH.Modules.Audit.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.Audit.Extensions;

public static class AuditModuleExtensions
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddScoped<IBeneficiaryAuditLogRepository, BeneficiaryAuditLogRepository>();
        return services;
    }

    public static IEndpointRouteBuilder MapAuditModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuditEndpoints();
        return endpoints;
    }
}
