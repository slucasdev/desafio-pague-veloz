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
    public class CancelarReservaCommandHandler : IRequestHandler<CancelarReservaCommand, OperationResult<TransacaoDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CancelarReservaCommandHandler> _logger;
        private readonly IMapper _mapper;

        public CancelarReservaCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CancelarReservaCommandHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<TransacaoDTO>> Handle(
            CancelarReservaCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando cancelamento de reserva na conta: {ContaId}, Valor: {Valor}, TransacaoReservaId: {TransacaoReservaId}",
                    request.ContaId, request.Valor, request.TransacaoReservaId);

                var transacaoExistente = await _unitOfWork.Transacoes.ObterPorIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

                if (transacaoExistente != null)
                {
                    _logger.LogInformation("Transação já processada (idempotência): {IdempotencyKey}", request.IdempotencyKey);
                    return OperationResult<TransacaoDTO>.SuccessResult(_mapper.Map<TransacaoDTO>(transacaoExistente), "Transação já processada anteriormente");
                }

                var transacaoReserva = await _unitOfWork.Transacoes.ObterPorIdAsync(request.TransacaoReservaId, cancellationToken);

                if (transacaoReserva == null)
                {
                    _logger.LogWarning("Transação de reserva não encontrada: {TransacaoReservaId}", request.TransacaoReservaId);
                    return OperationResult<TransacaoDTO>.FailureResult("Transação de reserva não encontrada", "TransacaoReservaId inválido");
                }

                var conta = await _unitOfWork.Contas.ObterComLockAsync(request.ContaId, cancellationToken);

                if (conta == null)
                {
                    _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                    return OperationResult<TransacaoDTO>.FailureResult("Conta não encontrada", "ContaId inválido");
                }

                conta.CancelarReserva(request.Valor, request.TransacaoReservaId, request.Descricao, request.IdempotencyKey);

                _unitOfWork.Contas.Atualizar(conta);

                _logger.LogInformation("Cancelamento de reserva realizado com sucesso na conta: {ContaId}, Saldo disponível: {SaldoDisponivel}",
                    conta.Id, conta.SaldoDisponivel);

                var transacao = conta.Transacoes.FirstOrDefault(t => t.IdempotencyKey == request.IdempotencyKey);

                if (transacao == null)
                {
                    _logger.LogError("Transação de cancelamento não encontrada. IdempotencyKey: {IdempotencyKey}", request.IdempotencyKey);
                    return OperationResult<TransacaoDTO>.FailureResult("Erro ao processar cancelamento", "Transação não encontrada");
                }

                transacao.MarcarComoProcessada();

                //_unitOfWork.Transacoes.Atualizar(transacao);

                return OperationResult<TransacaoDTO>.SuccessResult(_mapper.Map<TransacaoDTO>(transacao), "Cancelamento de reserva realizado com sucesso");
            }
            catch (ContaBloqueadaException ex)
            {
                _logger.LogWarning(ex, "Conta bloqueada: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Conta bloqueada", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar reserva na conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Erro ao processar cancelamento de reserva", ex.Message);
            }
        }
    }
}