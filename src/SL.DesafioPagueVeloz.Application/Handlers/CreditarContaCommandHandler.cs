using AutoMapper;
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
        private readonly IMapper _mapper;

        public CreditarContaCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CreditarContaCommandHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<TransacaoDTO>> Handle(
            CreditarContaCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando crédito na conta: {ContaId}, Valor: {Valor}",
                    request.ContaId, request.Valor);

                var transacaoExistente = await _unitOfWork.Transacoes.ObterPorIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

                if (transacaoExistente != null)
                {
                    _logger.LogInformation("Transação já processada (idempotência): {IdempotencyKey}", request.IdempotencyKey);
                    return OperationResult<TransacaoDTO>.SuccessResult(_mapper.Map<TransacaoDTO>(transacaoExistente), "Transação já processada anteriormente");
                }

                var conta = await _unitOfWork.Contas.ObterComLockAsync(request.ContaId, cancellationToken);

                if (conta == null)
                {
                    _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                    return OperationResult<TransacaoDTO>.FailureResult("Conta não encontrada", "ContaId inválido");
                }

                conta.Creditar(request.Valor, request.Descricao, request.IdempotencyKey);

                _unitOfWork.Contas.Atualizar(conta);

                _logger.LogInformation("Crédito realizado com sucesso na conta: {ContaId}, Novo saldo: {Saldo}",
                    conta.Id, conta.SaldoDisponivel);

                var transacao = conta.Transacoes.FirstOrDefault(t => t.IdempotencyKey == request.IdempotencyKey);

                if (transacao == null)
                {
                    _logger.LogError("Transação de crédito não encontrada. IdempotencyKey: {IdempotencyKey}", request.IdempotencyKey);
                    return OperationResult<TransacaoDTO>.FailureResult("Erro ao processar crédito", "Transação não encontrada");
                }

                transacao.MarcarComoProcessada();

                //_unitOfWork.Transacoes.Atualizar(transacao);

                return OperationResult<TransacaoDTO>.SuccessResult(_mapper.Map<TransacaoDTO>(transacao), "Crédito realizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao creditar conta: {ContaId}", request.ContaId);
                return OperationResult<TransacaoDTO>.FailureResult("Erro ao processar crédito", ex.Message);
            }
        }
    }
}