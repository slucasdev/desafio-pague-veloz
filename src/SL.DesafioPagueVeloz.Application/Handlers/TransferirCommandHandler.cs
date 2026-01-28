using AutoMapper;
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
        private readonly IMapper _mapper;

        public TransferirCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<TransferirCommandHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<List<TransacaoDTO>>> Handle(
            TransferirCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando transferência - Origem: {ContaOrigemId}, Destino: {ContaDestinoId}, Valor: {Valor}",
                    request.ContaOrigemId, request.ContaDestinoId, request.Valor);

                var transacaoExistente = await _unitOfWork.Transacoes.ObterPorIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

                if (transacaoExistente != null)
                {
                    _logger.LogInformation("Transação já processada (idempotência): {IdempotencyKey}", request.IdempotencyKey);

                    var transacoesRelacionadas = await _unitOfWork.Transacoes.ObterPorContaIdAsync(request.ContaOrigemId, cancellationToken);

                    var transacoesDTO = transacoesRelacionadas
                        .Where(t => t.IdempotencyKey == request.IdempotencyKey || t.TransacaoOrigemId == transacaoExistente.Id)
                        .Select(t => _mapper.Map<TransacaoDTO>(t))
                        .ToList();

                    return OperationResult<List<TransacaoDTO>>.SuccessResult(transacoesDTO, "Transação já processada anteriormente");
                }

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var contaOrigem = await _unitOfWork.Contas.ObterComLockAsync(request.ContaOrigemId, cancellationToken);
                    var contaDestino = await _unitOfWork.Contas.ObterComLockAsync(request.ContaDestinoId, cancellationToken);

                    if (contaOrigem == null)
                    {
                        _logger.LogWarning("Conta origem não encontrada: {ContaOrigemId}", request.ContaOrigemId);
                        return OperationResult<List<TransacaoDTO>>.FailureResult("Conta origem não encontrada", "ContaOrigemId inválido");
                    }

                    if (contaDestino == null)
                    {
                        _logger.LogWarning("Conta destino não encontrada: {ContaDestinoId}", request.ContaDestinoId);
                        return OperationResult<List<TransacaoDTO>>.FailureResult("Conta destino não encontrada", "ContaDestinoId inválido");
                    }

                    var idempotencyKeyDebito = request.IdempotencyKey;
                    var idempotencyKeyCredito = Guid.NewGuid();

                    contaOrigem.Debitar(request.Valor, $"Transferência para conta {contaDestino.Numero} - {request.Descricao}", idempotencyKeyDebito);
                    contaDestino.Creditar(request.Valor, $"Transferência da conta {contaOrigem.Numero} - {request.Descricao}", idempotencyKeyCredito);

                    _unitOfWork.Contas.Atualizar(contaOrigem);
                    _unitOfWork.Contas.Atualizar(contaDestino);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    _logger.LogInformation("Transferência realizada com sucesso - Origem: {ContaOrigemId}, Destino: {ContaDestinoId}",
                        request.ContaOrigemId, request.ContaDestinoId);

                    var transacaoDebito = contaOrigem.Transacoes.Last();
                    var transacaoCredito = contaDestino.Transacoes.Last();

                    transacaoDebito.MarcarComoProcessada();
                    transacaoCredito.MarcarComoProcessada();

                    _unitOfWork.Transacoes.Atualizar(transacaoDebito);
                    _unitOfWork.Transacoes.Atualizar(transacaoCredito);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    var resultado = new List<TransacaoDTO>
                    {
                        _mapper.Map<TransacaoDTO>(transacaoDebito),
                        _mapper.Map<TransacaoDTO>(transacaoCredito)
                    };

                    return OperationResult<List<TransacaoDTO>>.SuccessResult(resultado, "Transferência realizada com sucesso");

                }, cancellationToken);
            }
            catch (SaldoInsuficienteException ex)
            {
                _logger.LogWarning(ex, "Saldo insuficiente na conta origem: {ContaOrigemId}", request.ContaOrigemId);
                return OperationResult<List<TransacaoDTO>>.FailureResult("Saldo insuficiente na conta origem", ex.Message);
            }
            catch (ContaBloqueadaException ex)
            {
                _logger.LogWarning(ex, "Uma das contas está bloqueada");
                return OperationResult<List<TransacaoDTO>>.FailureResult("Conta bloqueada", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar transferência");
                return OperationResult<List<TransacaoDTO>>.FailureResult("Erro ao processar transferência", ex.Message);
            }
        }
    }
}