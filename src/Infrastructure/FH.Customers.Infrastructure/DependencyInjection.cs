using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Application.Messaging;
using FH.Customers.Infrastructure.CustomerPlans.Gateways;
using FH.Customers.Infrastructure.Messaging;
using FH.Customers.Infrastructure.MockData;

namespace FH.Customers.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la capa Infrastructure: adaptadores a sistemas externos (gateways), almacén mock
    /// en memoria y el publicador de eventos (en memoria por ahora; Kafka pendiente).
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Almacén Mock en memoria (permite probar la API sin PostgreSQL levantado)
        services.AddSingleton<InMemoryCustomerStore>();

        // Gateways y adaptadores de puertos
        services.AddScoped<ICustomerExistenceChecker, CustomerExistenceChecker>();
        services.AddScoped<IFinancialsPlanGateway, FinancialsPlanGatewayStub>();
        services.AddScoped<IAssignedBeneficiariesCounter, AssignedBeneficiariesCounterStub>();

        // Mensajería: publicador en memoria (registra en el log). Kafka se conecta cambiando esta línea.
        services.AddScoped<IEventPublisher, InMemoryEventPublisher>();

        return services;
    }
}