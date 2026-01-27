using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Exceptions;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class EstornarCommandHandler : IRequestHandler<EstornarCommand, OperationResult<TransacaoDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EstornarCommandHandler> _logger;

        public EstornarCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<EstornarCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OperationResult<TransacaoDTO>> Handle(
            EstornarCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando estorno na conta: {ContaId}, Valor: {Valor}, TransacaoOriginalId: {TransacaoOriginalId}",
                    request.ContaId, request.Valor, request.TransacaoOriginalId);

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

                // Verificar se a transação original existe
                var transacaoOriginal = await _unitOfWork.Transacoes
                    .ObterPorIdAsync(request.TransacaoOriginalId, cancellationToken);

                if (transacaoOriginal == null)
                {
                    _logger.LogWarning("Transação original não encontrada: {TransacaoOriginalId}", request.TransacaoOriginalId);
                    return OperationResult<TransacaoDTO>.FailureResult(
                        "Transação original não encontrada",
                        "TransacaoOriginalId inválido");
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

                // Executar operação de estorno
                conta.Estornar(request.Valor, request.TransacaoOriginalId, request.Descricao, request.IdempotencyKey);

                // Marcar transação original como estornada
                transacaoOriginal.MarcarComoEstornada();
                _unitOfWork.Transacoes.Atualizar(transacaoOriginal);

                _unitOfWork.Contas.Atualizar(conta);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Estorno realizado com sucesso na conta: {ContaId}, Novo saldo: {SaldoDisponivel}",
                    conta.Id, conta.SaldoDisponivel);

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

                return OperationResult<TransacaoDTO>.SuccessResult(resultado, "Estorno realizado com sucesso");
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
                _logger.LogError(ex, "Erro ao estornar transação na conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult(
                    "Erro ao processar estorno",
                    ex.Message);
            }
        }
    }
}
