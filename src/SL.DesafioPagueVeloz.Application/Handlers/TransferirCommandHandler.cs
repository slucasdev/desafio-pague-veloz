using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Exceptions;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class TransferirCommandHandler : IRequestHandler<TransferirCommand, OperationResult<List<TransacaoDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransferirCommandHandler> _logger;

        public TransferirCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<TransferirCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OperationResult<List<TransacaoDTO>>> Handle(
            TransferirCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando transferência - Origem: {ContaOrigemId}, Destino: {ContaDestinoId}, Valor: {Valor}",
                    request.ContaOrigemId, request.ContaDestinoId, request.Valor);

                // Verificar idempotência
                var transacaoExistente = await _unitOfWork.Transacoes
                    .ObterPorIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

                if (transacaoExistente != null)
                {
                    _logger.LogInformation("Transação já processada (idempotência): {IdempotencyKey}", request.IdempotencyKey);

                    // Buscar todas as transações relacionadas (débito e crédito)
                    var transacoesRelacionadas = await _unitOfWork.Transacoes
                        .ObterPorContaIdAsync(request.ContaOrigemId, cancellationToken);

                    var transacoesDTO = transacoesRelacionadas
                        .Where(t => t.IdempotencyKey == request.IdempotencyKey || t.TransacaoOrigemId == transacaoExistente.Id)
                        .Select(t => new TransacaoDTO
                        {
                            Id = t.Id,
                            ContaId = t.ContaId,
                            Tipo = t.Tipo.ToString(),
                            Valor = t.Valor,
                            Descricao = t.Descricao,
                            Status = t.Status.ToString(),
                            IdempotencyKey = t.IdempotencyKey,
                            TransacaoOrigemId = t.TransacaoOrigemId,
                            ProcessadoEm = t.ProcessadoEm,
                            MotivoFalha = t.MotivoFalha,
                            CriadoEm = t.CriadoEm
                        })
                        .ToList();

                    return OperationResult<List<TransacaoDTO>>.SuccessResult(transacoesDTO, "Transação já processada anteriormente");
                }

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Obter contas com lock pessimista
                var contaOrigem = await _unitOfWork.Contas.ObterComLockAsync(request.ContaOrigemId, cancellationToken);
                var contaDestino = await _unitOfWork.Contas.ObterComLockAsync(request.ContaDestinoId, cancellationToken);

                if (contaOrigem == null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    _logger.LogWarning("Conta origem não encontrada: {ContaOrigemId}", request.ContaOrigemId);
                    return OperationResult<List<TransacaoDTO>>.FailureResult(
                        "Conta origem não encontrada",
                        "ContaOrigemId inválido");
                }

                if (contaDestino == null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    _logger.LogWarning("Conta destino não encontrada: {ContaDestinoId}", request.ContaDestinoId);
                    return OperationResult<List<TransacaoDTO>>.FailureResult(
                        "Conta destino não encontrada",
                        "ContaDestinoId inválido");
                }

                // Gerar IdempotencyKeys únicos para cada transação
                var idempotencyKeyDebito = request.IdempotencyKey;
                var idempotencyKeyCredito = Guid.NewGuid();

                // Executar débito na conta origem
                contaOrigem.Debitar(
                    request.Valor,
                    $"Transferência para conta {contaDestino.Numero} - {request.Descricao}",
                    idempotencyKeyDebito);

                // Executar crédito na conta destino
                contaDestino.Creditar(
                    request.Valor,
                    $"Transferência da conta {contaOrigem.Numero} - {request.Descricao}",
                    idempotencyKeyCredito);

                _unitOfWork.Contas.Atualizar(contaOrigem);
                _unitOfWork.Contas.Atualizar(contaDestino);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Transferência realizada com sucesso - Origem: {ContaOrigemId}, Destino: {ContaDestinoId}",
                    request.ContaOrigemId, request.ContaDestinoId);

                // Obter as transações criadas
                var transacaoDebito = contaOrigem.Transacoes.Last();
                var transacaoCredito = contaDestino.Transacoes.Last();

                // Marcar transações como processadas
                transacaoDebito.MarcarComoProcessada();
                transacaoCredito.MarcarComoProcessada();

                _unitOfWork.Transacoes.Atualizar(transacaoDebito);
                _unitOfWork.Transacoes.Atualizar(transacaoCredito);
                await _unitOfWork.CommitAsync(cancellationToken);

                // Mapear para DTOs
                var resultado = new List<TransacaoDTO>
                {
                    new TransacaoDTO
                    {
                        Id = transacaoDebito.Id,
                        ContaId = transacaoDebito.ContaId,
                        Tipo = transacaoDebito.Tipo.ToString(),
                        Valor = transacaoDebito.Valor,
                        Descricao = transacaoDebito.Descricao,
                        Status = transacaoDebito.Status.ToString(),
                        IdempotencyKey = transacaoDebito.IdempotencyKey,
                        TransacaoOrigemId = transacaoDebito.TransacaoOrigemId,
                        ProcessadoEm = transacaoDebito.ProcessadoEm,
                        MotivoFalha = transacaoDebito.MotivoFalha,
                        CriadoEm = transacaoDebito.CriadoEm
                    },
                    new TransacaoDTO
                    {
                        Id = transacaoCredito.Id,
                        ContaId = transacaoCredito.ContaId,
                        Tipo = transacaoCredito.Tipo.ToString(),
                        Valor = transacaoCredito.Valor,
                        Descricao = transacaoCredito.Descricao,
                        Status = transacaoCredito.Status.ToString(),
                        IdempotencyKey = transacaoCredito.IdempotencyKey,
                        TransacaoOrigemId = transacaoCredito.TransacaoOrigemId,
                        ProcessadoEm = transacaoCredito.ProcessadoEm,
                        MotivoFalha = transacaoCredito.MotivoFalha,
                        CriadoEm = transacaoCredito.CriadoEm
                    }
                };

                return OperationResult<List<TransacaoDTO>>.SuccessResult(resultado, "Transferência realizada com sucesso");
            }
            catch (SaldoInsuficienteException ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Saldo insuficiente na conta origem: {ContaOrigemId}", request.ContaOrigemId);
                return OperationResult<List<TransacaoDTO>>.FailureResult(
                    "Saldo insuficiente na conta origem",
                    ex.Message);
            }
            catch (ContaBloqueadaException ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Uma das contas está bloqueada");
                return OperationResult<List<TransacaoDTO>>.FailureResult(
                    "Conta bloqueada",
                    ex.Message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Erro ao processar transferência");
                return OperationResult<List<TransacaoDTO>>.FailureResult(
                    "Erro ao processar transferência",
                    ex.Message);
            }
        }
    }
}
