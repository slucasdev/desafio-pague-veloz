using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class CreditarContaCommandHandler : IRequestHandler<CreditarContaCommand, OperationResult<TransacaoDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreditarContaCommandHandler> _logger;

        public CreditarContaCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CreditarContaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OperationResult<TransacaoDTO>> Handle(
            CreditarContaCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando crédito na conta: {ContaId}, Valor: {Valor}",
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

                // Executar operação de crédito
                conta.Creditar(request.Valor, request.Descricao, request.IdempotencyKey);

                _unitOfWork.Contas.Atualizar(conta);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Crédito realizado com sucesso na conta: {ContaId}, Novo saldo: {Saldo}",
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

                return OperationResult<TransacaoDTO>.SuccessResult(resultado, "Crédito realizado com sucesso");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Erro ao creditar conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult(
                    "Erro ao processar crédito",
                    ex.Message);
            }
        }
    }
}
