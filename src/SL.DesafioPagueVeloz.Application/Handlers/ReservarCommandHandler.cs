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
    public class ReservarCommandHandler : IRequestHandler<ReservarCommand, OperationResult<TransacaoDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReservarCommandHandler> _logger;
        private readonly IMapper _mapper;

        public ReservarCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ReservarCommandHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<TransacaoDTO>> Handle(
            ReservarCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando reserva na conta: {ContaId}, Valor: {Valor}", request.ContaId, request.Valor);

                var transacaoExistente = await _unitOfWork.Transacoes.ObterPorIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

                if (transacaoExistente != null)
                {
                    _logger.LogInformation("Transação já processada (idempotência): {IdempotencyKey}", request.IdempotencyKey);
                    return OperationResult<TransacaoDTO>.SuccessResult(_mapper.Map<TransacaoDTO>(transacaoExistente), "Transação já processada anteriormente");
                }

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var conta = await _unitOfWork.Contas.ObterComLockAsync(request.ContaId, cancellationToken);

                    if (conta == null)
                    {
                        _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                        return OperationResult<TransacaoDTO>.FailureResult("Conta não encontrada", "ContaId inválido");
                    }

                    conta.Reservar(request.Valor, request.Descricao, request.IdempotencyKey);

                    _unitOfWork.Contas.Atualizar(conta);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    _logger.LogInformation("Reserva realizada com sucesso na conta: {ContaId}, Saldo reservado: {SaldoReservado}", conta.Id, conta.SaldoReservado);

                    var transacao = conta.Transacoes.FirstOrDefault(t => t.IdempotencyKey == request.IdempotencyKey);

                    if (transacao == null)
                    {
                        _logger.LogError("Transação de reserva não encontrada. IdempotencyKey: {IdempotencyKey}", request.IdempotencyKey);
                        return OperationResult<TransacaoDTO>.FailureResult("Erro ao processar reserva", "Transação não encontrada");
                    }

                    transacao.MarcarComoProcessada();

                    _unitOfWork.Transacoes.Atualizar(transacao);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    return OperationResult<TransacaoDTO>.SuccessResult(_mapper.Map<TransacaoDTO>(transacao), "Reserva realizada com sucesso");

                }, cancellationToken);
            }
            catch (SaldoInsuficienteException ex)
            {
                _logger.LogWarning(ex, "Saldo insuficiente para reserva na conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Saldo insuficiente para reserva", ex.Message);
            }
            catch (ContaBloqueadaException ex)
            {
                _logger.LogWarning(ex, "Conta bloqueada: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Conta bloqueada", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reservar valor na conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Erro ao processar reserva", ex.Message);
            }
        }
    }
}