using FH.Modules.Audit.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.Audit.Extensions;

public static class AuditModuleExtensions
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Registro de servicios de bitácora y auditoría
        return services;
    }

    public static IEndpointRouteBuilder MapAuditModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuditEndpoints();
        return endpoints;
    }
}
