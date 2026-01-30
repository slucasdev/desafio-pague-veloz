using SL.DesafioPagueVeloz.Application.Interfaces;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;
using SL.DesafioPagueVeloz.Infrastructure.BackgroundServices;
using SL.DesafioPagueVeloz.Infrastructure.Messaging;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Uow;

namespace SL.DesafioPagueVeloz.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IContaRepository, ContaRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventPublisher>();

        // Background Services
        services.AddHostedService<OutboxProcessorService>();

        return services;
    }
}