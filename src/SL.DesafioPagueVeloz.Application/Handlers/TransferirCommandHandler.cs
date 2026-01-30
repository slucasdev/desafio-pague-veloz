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

                    var resultIdempotente = new List<TransacaoDTO>();
                    resultIdempotente.Add(_mapper.Map<TransacaoDTO>(transacaoExistente));

                    var transacoesDestino = await _unitOfWork.Transacoes
                        .ObterPorContaIdAsync(request.ContaDestinoId, cancellationToken);

                    var creditoCorrespondente = transacoesDestino
                        .Where(t => t.Tipo == Domain.Enums.TipoOperacao.Credito
                                 && t.Valor == request.Valor
                                 && t.CriadoEm >= transacaoExistente.CriadoEm.AddSeconds(-5)
                                 && t.CriadoEm <= transacaoExistente.CriadoEm.AddSeconds(5))
                        .OrderByDescending(t => t.CriadoEm)
                        .FirstOrDefault();

                    if (creditoCorrespondente != null)
                    {
                        resultIdempotente.Add(_mapper.Map<TransacaoDTO>(creditoCorrespondente));
                    }

                    return OperationResult<List<TransacaoDTO>>.SuccessResult(resultIdempotente, "Transação já processada anteriormente");
                }

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

                _logger.LogInformation("Transferência realizada com sucesso - Origem: {ContaOrigemId}, Destino: {ContaDestinoId}",
                    request.ContaOrigemId, request.ContaDestinoId);

                var transacaoDebito = contaOrigem.Transacoes.FirstOrDefault(t => t.IdempotencyKey == idempotencyKeyDebito);
                var transacaoCreditoNova = contaDestino.Transacoes.FirstOrDefault(t => t.IdempotencyKey == idempotencyKeyCredito);

                if (transacaoDebito == null || transacaoCreditoNova == null)
                {
                    _logger.LogError("Transações de transferência não encontradas. DebitoKey: {DebitoKey}, CreditoKey: {CreditoKey}",
                        idempotencyKeyDebito, idempotencyKeyCredito);

                    return OperationResult<List<TransacaoDTO>>.FailureResult("Erro ao processar transferência", "Transações não encontradas");
                }

                transacaoDebito.MarcarComoProcessada();
                transacaoCreditoNova.MarcarComoProcessada();

                //_unitOfWork.Transacoes.Atualizar(transacaoDebito);
                //_unitOfWork.Transacoes.Atualizar(transacaoCreditoNova);

                var resultado = new List<TransacaoDTO>
                {
                    _mapper.Map<TransacaoDTO>(transacaoDebito),
                    _mapper.Map<TransacaoDTO>(transacaoCreditoNova)
                };

                return OperationResult<List<TransacaoDTO>>.SuccessResult(
                    resultado,
                    "Transferência realizada com sucesso");
            }
            catch (SaldoInsuficienteException ex)
            {
                _logger.LogWarning(ex, "Saldo insuficiente na conta origem: {ContaOrigemId}", request.ContaOrigemId);
                return OperationResult<List<TransacaoDTO>>.FailureResult(
                    "Saldo insuficiente na conta origem",
                    ex.Message);
            }
            catch (ContaBloqueadaException ex)
            {
                _logger.LogWarning(ex, "Uma das contas está bloqueada");
                return OperationResult<List<TransacaoDTO>>.FailureResult(
                    "Conta bloqueada",
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar transferência");
                return OperationResult<List<TransacaoDTO>>.FailureResult(
                    "Erro ao processar transferência",
                    ex.Message);
            }
        }
    }
}