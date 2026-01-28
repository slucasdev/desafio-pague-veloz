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
    public class EstornarCommandHandler : IRequestHandler<EstornarCommand, OperationResult<TransacaoDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EstornarCommandHandler> _logger;
        private readonly IMapper _mapper;

        public EstornarCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<EstornarCommandHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<TransacaoDTO>> Handle(
            EstornarCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando estorno na conta: {ContaId}, Valor: {Valor}, TransacaoOriginalId: {TransacaoOriginalId}",
                    request.ContaId, request.Valor, request.TransacaoOriginalId);

                var transacaoExistente = await _unitOfWork.Transacoes.ObterPorIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

                if (transacaoExistente != null)
                {
                    _logger.LogInformation("Transação já processada (idempotência): {IdempotencyKey}", request.IdempotencyKey);
                    return OperationResult<TransacaoDTO>.SuccessResult(_mapper.Map<TransacaoDTO>(transacaoExistente), "Transação já processada anteriormente");
                }

                var transacaoOriginal = await _unitOfWork.Transacoes.ObterPorIdAsync(request.TransacaoOriginalId, cancellationToken);

                if (transacaoOriginal == null)
                {
                    _logger.LogWarning("Transação original não encontrada: {TransacaoOriginalId}", request.TransacaoOriginalId);
                    return OperationResult<TransacaoDTO>.FailureResult("Transação original não encontrada", "TransacaoOriginalId inválido");
                }

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var conta = await _unitOfWork.Contas.ObterComLockAsync(request.ContaId, cancellationToken);

                    if (conta == null)
                    {
                        _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                        return OperationResult<TransacaoDTO>.FailureResult("Conta não encontrada", "ContaId inválido");
                    }

                    conta.Estornar(request.Valor, request.TransacaoOriginalId, request.Descricao, request.IdempotencyKey);

                    transacaoOriginal.MarcarComoEstornada();
                    _unitOfWork.Transacoes.Atualizar(transacaoOriginal);

                    _unitOfWork.Contas.Atualizar(conta);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    _logger.LogInformation("Estorno realizado com sucesso na conta: {ContaId}, Novo saldo: {SaldoDisponivel}",
                        conta.Id, conta.SaldoDisponivel);

                    var transacao = conta.Transacoes.Last();
                    transacao.MarcarComoProcessada();

                    _unitOfWork.Transacoes.Atualizar(transacao);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    return OperationResult<TransacaoDTO>.SuccessResult(_mapper.Map<TransacaoDTO>(transacao), "Estorno realizado com sucesso");

                }, cancellationToken);
            }
            catch (ContaBloqueadaException ex)
            {
                _logger.LogWarning(ex, "Conta bloqueada: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Conta bloqueada", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao estornar transação na conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Erro ao processar estorno", ex.Message);
            }
        }
    }
}