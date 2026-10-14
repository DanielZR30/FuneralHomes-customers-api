using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FH.Customers.Application.UseCases.CustomerPlans;
using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Infrastructure.CustomerPlans.Gateways;
using FH.Customers.Infrastructure.Messaging;
using FH.Customers.Application.Contracts.Gateways;
using FH.Customers.Application.Contracts.Messaging;

namespace FH.Customers.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la capa Infrastructure: adaptadores a sistemas externos (gateways)
    /// y el publicador de eventos (en memoria por ahora; Kafka pendiente).
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Gateways: puertos hacia otros microservicios (Financials). Lo que vive en nuestra
        // propia base de datos se consulta con repositorios, no con gateways.
        services.AddScoped<IFinancialsPlanGateway, FinancialsPlanGatewayStub>();

        // Mensajería: publicador en memoria (registra en el log). Kafka se conecta cambiando esta línea.
        services.AddScoped<IEventPublisher, InMemoryEventPublisher>();

        return services;
    }
}