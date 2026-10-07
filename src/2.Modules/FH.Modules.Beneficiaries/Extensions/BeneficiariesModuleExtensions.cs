using FH.Modules.Beneficiaries.Domain.Repositories;
using FH.Modules.Beneficiaries.Infrastructure.Endpoints;
using FH.Modules.Beneficiaries.Infrastructure.Persistence;
using FH.Modules.Beneficiaries.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.Beneficiaries.Extensions;

public static class BeneficiariesModuleExtensions
{
    public static IServiceCollection AddBeneficiariesModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var connectionString = configuration?.GetConnectionString("CustomerDb")
            ?? "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret";

        services.AddDbContext<BeneficiariesDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(BeneficiariesDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        return services;
    }

    public static IEndpointRouteBuilder MapBeneficiariesModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapBeneficiaryEndpoints();
        return endpoints;
    }
}
