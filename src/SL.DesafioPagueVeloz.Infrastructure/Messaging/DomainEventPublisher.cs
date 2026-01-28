using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SL.DesafioPagueVeloz.Application.Interfaces;
using SL.DesafioPagueVeloz.Domain.Events;
using SL.DesafioPagueVeloz.Domain.Events.Cliente;
using SL.DesafioPagueVeloz.Domain.Events.Conta;
using SL.DesafioPagueVeloz.Domain.Events.Transacao;

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
                        _logger.LogInformation("Processando evento {EventType} com ID {EventId}", domainEvent.TipoEvento, domainEvent.EventId);

                        // Processar evento (sem message broker)
                        await ProcessarEventoAsync(domainEvent, cancellationToken);

                        // OBS: Aqui poderíamos publicar no RabbitMQ, Kafka, etc (mas não faz parte dos requisitos do desafio)
                        // ex com RabbitMQ:
                        //await _rabbitMqPublisher.PublishAsync(domainEvent, cancellationToken);

                        _logger.LogInformation("Evento {EventType} com ID {EventId} processado com sucesso", domainEvent.TipoEvento, domainEvent.EventId);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao processar evento {EventType} com ID {EventId} após todas as tentativas", domainEvent.TipoEvento, domainEvent.EventId);

                    // Lançando exceção para que OutboxProcessorService registre a falha
                    throw;
                }
            }
        }

        private async Task ProcessarEventoAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            // Processamento baseado no tipo de evento
            switch (domainEvent)
            {
                case TransacaoCriadaEvent transacaoEvent:
                    await ProcessarTransacaoCriada(transacaoEvent);
                    break;

                case TransacaoProcessadaEvent processadaEvent:
                    await ProcessarTransacaoProcessada(processadaEvent);
                    break;

                case TransacaoFalhouEvent falhaEvent:
                    await ProcessarTransacaoFalhou(falhaEvent);
                    break;

                case SaldoAtualizadoEvent saldoEvent:
                    await ProcessarSaldoAtualizado(saldoEvent);
                    break;

                case TransferenciaRealizadaEvent transferenciaEvent:
                    await ProcessarTransferenciaRealizada(transferenciaEvent);
                    break;

                case ClienteCriadoEvent clienteEvent:
                    await ProcessarClienteCriado(clienteEvent);
                    break;

                case ClienteDesativadoEvent clienteDesativadoEvent:
                    await ProcessarClienteDesativado(clienteDesativadoEvent);
                    break;

                case ContaCriadaEvent contaEvent:
                    await ProcessarContaCriada(contaEvent);
                    break;

                case ContaBloqueadaEvent contaBloqueadaEvent:
                    await ProcessarContaBloqueada(contaBloqueadaEvent);
                    break;

                default:
                    _logger.LogInformation("Evento {EventType} processado (sem ação específica)", domainEvent.TipoEvento);
                    break;
            }

            await Task.CompletedTask;
        }

        #region Processadores de Eventos Específicos

        private async Task ProcessarTransacaoCriada(TransacaoCriadaEvent evento)
        {
            _logger.LogInformation("Transação criada - ID: {TransacaoId}, Conta: {ContaId}, Tipo: {TipoOperacao}, Valor: {Valor:C}, Descrição: {Descricao}",
                evento.TransacaoId,
                evento.ContaId,
                evento.TipoOperacao.ToString(),
                evento.Valor,
                evento.Descricao);

            // **** Exemplos de uso para este processador: ****
            // - Enviar notificação push
            // - Enviar email de confirmação
            // - Atualizar dashboard em tempo real (SignalR)
            // - Registrar em sistema de auditoria externo

            await Task.CompletedTask;
        }

        private async Task ProcessarTransacaoProcessada(TransacaoProcessadaEvent evento)
        {
            _logger.LogInformation("Transação processada - ID: {TransacaoId}, Conta: {ContaId}, Tipo: {TipoOperacao}, Valor: {Valor:C}, Saldo Disponível: {SaldoDisponivel:C}, Saldo Reservado: {SaldoReservado:C}",
                evento.TransacaoId,
                evento.ContaId,
                evento.TipoOperacao.ToString(),
                evento.Valor,
                evento.SaldoDisponivel,
                evento.SaldoReservado);

            // **** Exemplos de uso para este processador: ****
            // - Atualizar status em dashboard
            // - Enviar confirmação final ao cliente
            // - Registrar conclusão em sistema de monitoramento

            await Task.CompletedTask;
        }

        private async Task ProcessarTransacaoFalhou(TransacaoFalhouEvent evento)
        {
            _logger.LogWarning("Transação falhou - ID: {TransacaoId}, Motivo: {MotivoFalha}",
                evento.TransacaoId,
                evento.MotivoFalha);

            // **** Exemplos de uso para este processador: ****
            // - Enviar alerta para suporte
            // - Notificar cliente sobre falha
            // - Registrar em sistema de incidentes
            // - Criar ticket automático

            await Task.CompletedTask;
        }

        private async Task ProcessarSaldoAtualizado(SaldoAtualizadoEvent evento)
        {
            _logger.LogInformation(
                "Saldo atualizado - Conta: {ContaId} | " +
                "Saldo Disponível: {SaldoAnterior:C} → {SaldoAtual:C} | " +
                "Saldo Reservado: {ReservadoAnterior:C} → {ReservadoAtual:C} | " +
                "Diferença Total: {Diferenca:C}",
                evento.ContaId,
                evento.SaldoDisponivelAnterior,
                evento.SaldoDisponivelAtual,
                evento.SaldoReservadoAnterior,
                evento.SaldoReservadoAtual,
                evento.Diferenca);

            // **** Exemplos de uso para este processador: ****
            // - Atualizar cache Redis com novo saldo
            // - Enviar notificação se saldo ficou negativo
            // - Atualizar dashboard em tempo real
            // - Registrar métrica para análise
            // - Verificar limites e alertas
            // - Disparar alertas de saldo baixo

            await Task.CompletedTask;
        }

        private async Task ProcessarTransferenciaRealizada(TransferenciaRealizadaEvent evento)
        {
            _logger.LogInformation("Transferência realizada - Origem: {ContaOrigemId}, Destino: {ContaDestinoId}, Valor: {Valor:C}",
                evento.ContaOrigemId,
                evento.ContaDestinoId,
                evento.Valor);

            // **** Exemplos de uso para este processador: ****
            // - Enviar notificação para AMBAS as contas
            // - Enviar para sistema de monitoramento de fraude
            // - Criar registro em sistema de auditoria

            await Task.CompletedTask;
        }

        private async Task ProcessarClienteCriado(ClienteCriadoEvent evento)
        {
            _logger.LogInformation("Cliente criado - ID: {ClienteId}, Nome: {Nome}, Email: {Email}, Documento: {Documento}",
                evento.ClienteId,
                evento.Nome,
                evento.Email,
                evento.Documento);

            // **** Exemplos de uso para este processador: ****
            // - Enviar email de boas-vindas
            // - Criar registro em CRM
            // - Enviar para fila de onboarding
            // - Iniciar processo de KYC (Know Your Customer)
            // - Registrar em sistema de marketing

            await Task.CompletedTask;
        }

        private async Task ProcessarClienteDesativado(ClienteDesativadoEvent evento)
        {
            _logger.LogWarning("Cliente desativado - ID: {ClienteId}, Motivo: {Motivo}",
                evento.ClienteId,
                evento.Motivo);

            // **** Exemplos de uso para este processador: ****
            // - Enviar email informando desativação
            // - Bloquear todas as contas do cliente
            // - Notificar equipe de suporte
            // - Registrar em sistema de compliance
            // - Cancelar serviços ativos (cartões, limites, etc)
            // - Arquivar dados para inativação
            // - Enviar notificação para sistemas dependentes

            await Task.CompletedTask;
        }

        private async Task ProcessarContaCriada(ContaCriadaEvent evento)
        {
            _logger.LogInformation("Conta criada - ID: {ContaId}, Cliente: {ClienteId}, Número: {Numero}, Limite: {LimiteCredito:C}",
                evento.ContaId,
                evento.ClienteId,
                evento.Numero,
                evento.LimiteCredito);

            // **** Exemplos de uso para este processador: ****
            // - Enviar notificação de nova conta criada
            // - Gerar cartão virtual
            // - Ativar serviços adicionais (PIX, boleto, etc)
            // - Registrar em sistema de billing
            // - Criar conta em sistemas auxiliares

            await Task.CompletedTask;
        }

        private async Task ProcessarContaBloqueada(ContaBloqueadaEvent evento)
        {
            _logger.LogWarning("Conta bloqueada - ID: {ContaId}, Motivo: {Motivo}",
                evento.ContaId,
                evento.Motivo);

            // **** Exemplos de uso para este processador: ****
            // - Enviar notificação URGENTE ao cliente
            // - Alertar equipe de segurança
            // - Registrar em sistema de compliance
            // - Bloquear cartões associados
            // - Criar ticket para análise

            await Task.CompletedTask;
        }

        #endregion
    }
}
