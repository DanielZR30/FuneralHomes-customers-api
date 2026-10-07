using FH.Modules.Customer.Domain.Repositories;
using FH.Modules.Customer.Infrastructure.Endpoints;
using FH.Modules.Customer.Infrastructure.Persistence;
using FH.Modules.Customer.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.Customer.Extensions;

public static class CustomerModuleExtensions
{
    public static IServiceCollection AddCustomerModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var connectionString = configuration?.GetConnectionString("CustomerDb")
            ?? "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret";

        services.AddDbContext<CustomerModuleDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(CustomerModuleDbContext).Assembly.FullName);
            });
        });

        // Los handlers de comandos y consultas se descubren por MediatR: el ensamblado
        // de este módulo se agrega en AddSharedCqrs (Program.cs).
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<FH.Modules.Customer.Application.Abstractions.ICustomerUnitOfWork, CustomerUnitOfWork>();

        return services;
    }

    public static IEndpointRouteBuilder MapCustomerModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCustomerEndpoints();
        return endpoints;
    }
}
