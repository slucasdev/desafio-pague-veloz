using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Queries;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class ObterContaPorIdQueryHandler : IRequestHandler<ObterContaPorIdQuery, OperationResult<ContaDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObterContaPorIdQueryHandler> _logger;

        public ObterContaPorIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<ObterContaPorIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OperationResult<ContaDTO>> Handle(
            ObterContaPorIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Buscando conta: {ContaId}", request.ContaId);

                var conta = await _unitOfWork.Contas.ObterPorIdAsync(request.ContaId, cancellationToken);

                if (conta == null)
                {
                    _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                    return OperationResult<ContaDTO>.FailureResult(
                        "Conta não encontrada",
                        "ContaId inválido");
                }

                var contaDTO = new ContaDTO
                {
                    Id = conta.Id,
                    ClienteId = conta.ClienteId,
                    Numero = conta.Numero,
                    SaldoDisponivel = conta.SaldoDisponivel,
                    SaldoReservado = conta.SaldoReservado,
                    LimiteCredito = conta.LimiteCredito,
                    SaldoTotal = conta.SaldoTotal,
                    Status = conta.Status.ToString(),
                    CriadoEm = conta.CriadoEm,
                    AtualizadoEm = conta.AtualizadoEm
                };

                _logger.LogInformation("Conta encontrada: {ContaId}", request.ContaId);

                return OperationResult<ContaDTO>.SuccessResult(contaDTO, "Conta encontrada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar conta: {ContaId}", request.ContaId);
                return OperationResult<ContaDTO>.FailureResult(
                    "Erro ao buscar conta",
                    ex.Message);
            }
        }
    }
}
