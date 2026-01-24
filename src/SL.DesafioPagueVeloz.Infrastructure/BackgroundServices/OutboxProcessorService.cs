using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.Interfaces;
using SL.DesafioPagueVeloz.Domain.Events;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;
using System.Text.Json;

namespace SL.DesafioPagueVeloz.Infrastructure.BackgroundServices
{
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessorService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(5);

        public OutboxProcessorService(
            IServiceProvider serviceProvider,
            ILogger<OutboxProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor Service iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessarMensagensAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagens da outbox");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }

            _logger.LogInformation("Outbox Processor Service finalizado");
        }

        private async Task ProcessarMensagensAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var eventDispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

            var mensagens = await unitOfWork.OutboxMessages
                .ObterMensagensNaoProcessadasAsync(100, cancellationToken);

            if (!mensagens.Any())
                return;

            _logger.LogInformation("Processando {Count} mensagens da outbox", mensagens.Count());

            foreach (var mensagem in mensagens)
            {
                try
                {
                    // Desserializar o evento
                    var tipoEvento = Type.GetType($"SL.DesafioPagueVeloz.Domain.Events.{mensagem.TipoEvento}, SL.DesafioPagueVeloz.Domain");

                    if (tipoEvento == null)
                    {
                        _logger.LogError("Tipo de evento não encontrado: {TipoEvento}", mensagem.TipoEvento);
                        mensagem.RegistrarTentativaFalha($"Tipo de evento não encontrado: {mensagem.TipoEvento}", TimeSpan.FromMinutes(5));
                        continue;
                    }

                    var domainEvent = JsonSerializer.Deserialize(mensagem.ConteudoJson, tipoEvento) as DomainEvent;

                    if (domainEvent == null)
                    {
                        _logger.LogError("Falha ao desserializar evento: {TipoEvento}", mensagem.TipoEvento);
                        mensagem.RegistrarTentativaFalha("Falha ao desserializar evento", TimeSpan.FromMinutes(5));
                        continue;
                    }

                    // Publicar o evento
                    await eventDispatcher.DispatchAsync(domainEvent, cancellationToken);

                    // Marcar como processado
                    mensagem.MarcarComoProcessado();
                    unitOfWork.OutboxMessages.Atualizar(mensagem);

                    _logger.LogInformation(
                        "Mensagem {MessageId} do tipo {TipoEvento} processada com sucesso",
                        mensagem.Id,
                        mensagem.TipoEvento);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Erro ao processar mensagem {MessageId} do tipo {TipoEvento}. Tentativa {Tentativa}",
                        mensagem.Id,
                        mensagem.TipoEvento,
                        mensagem.TentativasProcessamento + 1);

                    // Calcular delay com backoff exponencial
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, mensagem.TentativasProcessamento));
                    mensagem.RegistrarTentativaFalha(ex.Message, delay);
                    unitOfWork.OutboxMessages.Atualizar(mensagem);
                }
            }

            await unitOfWork.CommitAsync(cancellationToken);
        }
    }
}
