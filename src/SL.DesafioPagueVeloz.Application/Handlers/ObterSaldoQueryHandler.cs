using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Queries;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class ObterSaldoQueryHandler : IRequestHandler<ObterSaldoQuery, OperationResult<SaldoDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObterSaldoQueryHandler> _logger;
        private readonly IMapper _mapper;

        public ObterSaldoQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<ObterSaldoQueryHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<SaldoDTO>> Handle(
            ObterSaldoQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Consultando saldo da conta: {ContaId}", request.ContaId);

                var conta = await _unitOfWork.Contas.ObterPorIdAsync(request.ContaId, cancellationToken);

                if (conta == null)
                {
                    _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                    return OperationResult<SaldoDTO>.FailureResult("Conta não encontrada", "ContaId inválido");
                }

                var saldoDTO = _mapper.Map<SaldoDTO>(conta);
                saldoDTO.ConsultadoEm = DateTime.UtcNow;

                _logger.LogInformation("Saldo consultado com sucesso - Conta: {ContaId}, Saldo: {Saldo}", conta.Id, conta.SaldoDisponivel);

                return OperationResult<SaldoDTO>.SuccessResult(saldoDTO, "Saldo consultado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar saldo da conta: {ContaId}", request.ContaId);
                return OperationResult<SaldoDTO>.FailureResult("Erro ao consultar saldo", ex.Message);
            }
        }
    }
}