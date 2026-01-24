using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SL.DesafioPagueVeloz.Application.Interfaces;
using SL.DesafioPagueVeloz.Domain.Events;

namespace SL.DesafioPagueVeloz.Infrastructure.Messaging
{
    public class DomainEventPublisher : IDomainEventDispatcher
    {
        private readonly ILogger<DomainEventPublisher> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public DomainEventPublisher(ILogger<DomainEventPublisher> logger)
        {
            _logger = logger;

            // Configurar Polly com retry e backoff exponencial
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Tentativa {RetryCount} de publicar evento falhou. Aguardando {TimeSpan}ms antes de tentar novamente.",
                            retryCount,
                            timeSpan.TotalMilliseconds);
                    });
        }

        public async Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await DispatchAsync(new[] { domainEvent }, cancellationToken);
        }

        public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            foreach (var domainEvent in domainEvents)
            {
                try
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        _logger.LogInformation("Publicando evento {EventType} com ID {EventId}", domainEvent.TipoEvento, domainEvent.EventId);

                        // TODO: @slucasdev - Implementar a publicação para RabbitMQ
                        await PublicarParaMessageBrokerAsync(domainEvent, cancellationToken);

                        _logger.LogInformation("Evento {EventType} com ID {EventId} publicado com sucesso", domainEvent.TipoEvento, domainEvent.EventId);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao publicar evento {EventType} com ID {EventId} após todas as tentativas", domainEvent.TipoEvento, domainEvent.EventId);

                    // TODO: @slucasdev - lançando exception, porém, podemos salvar em uma Dead Letter Queue caso dê tempo
                    throw;
                }
            }
        }

        private async Task PublicarParaMessageBrokerAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            // Simulação de latência de rede (remover)
            await Task.Delay(100, cancellationToken);

            // TODO: @slucasdev - Implementar publicação de evento com RabbitMQ, EX:
            // await _rabbitMqPublisher.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
