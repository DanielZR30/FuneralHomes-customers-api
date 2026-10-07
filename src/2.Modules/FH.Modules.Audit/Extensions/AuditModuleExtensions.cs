using FH.Modules.Audit.Domain.Repositories;
using FH.Modules.Audit.Infrastructure.Endpoints;
using FH.Modules.Audit.Infrastructure.Persistence;
using FH.Modules.Audit.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.Audit.Extensions;

public static class AuditModuleExtensions
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var connectionString = configuration?.GetConnectionString("CustomerDb")
            ?? "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret";

        services.AddDbContext<AuditDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AuditDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IBeneficiaryAuditLogRepository, BeneficiaryAuditLogRepository>();
        services.AddScoped<FH.Modules.Audit.Domain.Abstractions.ISubscriptionExistenceChecker, FH.Modules.Audit.Infrastructure.Gateways.SqlSubscriptionExistenceChecker>();
        return services;
    }

    public static IEndpointRouteBuilder MapAuditModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuditEndpoints();
        return endpoints;
    }
}
