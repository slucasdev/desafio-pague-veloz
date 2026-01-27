using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Exceptions;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class ReservarCommandHandler : IRequestHandler<ReservarCommand, OperationResult<TransacaoDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReservarCommandHandler> _logger;

        public ReservarCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ReservarCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OperationResult<TransacaoDTO>> Handle(
            ReservarCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando reserva na conta: {ContaId}, Valor: {Valor}",
                    request.ContaId, request.Valor);

                // Verificar idempotência
                var transacaoExistente = await _unitOfWork.Transacoes
                    .ObterPorIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

                if (transacaoExistente != null)
                {
                    _logger.LogInformation("Transação já processada (idempotência): {IdempotencyKey}", request.IdempotencyKey);

                    var transacaoDTO = new TransacaoDTO
                    {
                        Id = transacaoExistente.Id,
                        ContaId = transacaoExistente.ContaId,
                        Tipo = transacaoExistente.Tipo.ToString(),
                        Valor = transacaoExistente.Valor,
                        Descricao = transacaoExistente.Descricao,
                        Status = transacaoExistente.Status.ToString(),
                        IdempotencyKey = transacaoExistente.IdempotencyKey,
                        TransacaoOrigemId = transacaoExistente.TransacaoOrigemId,
                        ProcessadoEm = transacaoExistente.ProcessadoEm,
                        MotivoFalha = transacaoExistente.MotivoFalha,
                        CriadoEm = transacaoExistente.CriadoEm
                    };

                    return OperationResult<TransacaoDTO>.SuccessResult(transacaoDTO, "Transação já processada anteriormente");
                }

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Obter conta com lock pessimista
                var conta = await _unitOfWork.Contas.ObterComLockAsync(request.ContaId, cancellationToken);

                if (conta == null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                    return OperationResult<TransacaoDTO>.FailureResult(
                        "Conta não encontrada",
                        "ContaId inválido");
                }

                // Executar operação de reserva
                conta.Reservar(request.Valor, request.Descricao, request.IdempotencyKey);

                _unitOfWork.Contas.Atualizar(conta);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Reserva realizada com sucesso na conta: {ContaId}, Saldo reservado: {SaldoReservado}",
                    conta.Id, conta.SaldoReservado);

                // Obter a transação criada
                var transacao = conta.Transacoes.Last();
                transacao.MarcarComoProcessada();
                _unitOfWork.Transacoes.Atualizar(transacao);
                await _unitOfWork.CommitAsync(cancellationToken);

                // Mapear para DTO
                var resultado = new TransacaoDTO
                {
                    Id = transacao.Id,
                    ContaId = transacao.ContaId,
                    Tipo = transacao.Tipo.ToString(),
                    Valor = transacao.Valor,
                    Descricao = transacao.Descricao,
                    Status = transacao.Status.ToString(),
                    IdempotencyKey = transacao.IdempotencyKey,
                    TransacaoOrigemId = transacao.TransacaoOrigemId,
                    ProcessadoEm = transacao.ProcessadoEm,
                    MotivoFalha = transacao.MotivoFalha,
                    CriadoEm = transacao.CriadoEm
                };

                return OperationResult<TransacaoDTO>.SuccessResult(resultado, "Reserva realizada com sucesso");
            }
            catch (SaldoInsuficienteException ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Saldo insuficiente para reserva na conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult(
                    "Saldo insuficiente para reserva",
                    ex.Message);
            }
            catch (ContaBloqueadaException ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Conta bloqueada: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult(
                    "Conta bloqueada",
                    ex.Message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Erro ao reservar valor na conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult(
                    "Erro ao processar reserva",
                    ex.Message);
            }
        }
    }
}
