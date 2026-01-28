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
    public class CapturarCommandHandler : IRequestHandler<CapturarCommand, OperationResult<TransacaoDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CapturarCommandHandler> _logger;
        private readonly IMapper _mapper;

        public CapturarCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CapturarCommandHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<TransacaoDTO>> Handle(
            CapturarCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando captura na conta: {ContaId}, Valor: {Valor}, TransacaoReservaId: {TransacaoReservaId}",
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

                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var conta = await _unitOfWork.Contas.ObterComLockAsync(request.ContaId, cancellationToken);

                    if (conta == null)
                    {
                        _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                        return OperationResult<TransacaoDTO>.FailureResult("Conta não encontrada", "ContaId inválido");
                    }

                    conta.Capturar(request.Valor, request.TransacaoReservaId, request.Descricao, request.IdempotencyKey);

                    _unitOfWork.Contas.Atualizar(conta);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    _logger.LogInformation("Captura realizada com sucesso na conta: {ContaId}, Saldo reservado: {SaldoReservado}", conta.Id, conta.SaldoReservado);

                    var transacao = conta.Transacoes.Last();
                    transacao.MarcarComoProcessada();

                    _unitOfWork.Transacoes.Atualizar(transacao);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    return OperationResult<TransacaoDTO>.SuccessResult(_mapper.Map<TransacaoDTO>(transacao), "Captura realizada com sucesso");

                }, cancellationToken);
            }
            catch (ContaBloqueadaException ex)
            {
                _logger.LogWarning(ex, "Conta bloqueada: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Conta bloqueada", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao capturar valor na conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Erro ao processar captura", ex.Message);
            }
        }
    }
}