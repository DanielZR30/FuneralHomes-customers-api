using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.CustomerPlans.Application;

public static class CustomerPlansApplicationServices
{
    public static IServiceCollection AddCustomerPlansApplicationServices(this IServiceCollection services)
    {
        // Registro de validadores FluentValidation
        services.AddValidatorsFromAssembly(typeof(CustomerPlansApplicationServices).Assembly);

        return services;
    }
}
