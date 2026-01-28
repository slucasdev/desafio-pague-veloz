using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Queries;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class ObterExtratoQueryHandler : IRequestHandler<ObterExtratoQuery, OperationResult<ExtratoDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObterExtratoQueryHandler> _logger;
        private readonly IMapper _mapper;

        public ObterExtratoQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<ObterExtratoQueryHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<ExtratoDTO>> Handle(
            ObterExtratoQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Gerando extrato - Conta: {ContaId}, Período: {DataInicio} a {DataFim}",
                    request.ContaId, request.DataInicio, request.DataFim);

                var conta = await _unitOfWork.Contas.ObterPorIdAsync(request.ContaId, cancellationToken);

                if (conta == null)
                {
                    _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                    return OperationResult<ExtratoDTO>.FailureResult("Conta não encontrada", "ContaId inválido");
                }

                var transacoes = await _unitOfWork.Transacoes.ObterPorContaIdEPeriodoAsync(
                    request.ContaId,
                    request.DataInicio,
                    request.DataFim,
                    cancellationToken);

                var transacoesDTO = _mapper.Map<List<TransacaoDTO>>(transacoes);

                // Calcular saldo inicial (transações antes do período)
                var transacoesAnteriores = await _unitOfWork.Transacoes.ObterPorContaIdEPeriodoAsync(
                    request.ContaId,
                    DateTime.MinValue,
                    request.DataInicio,
                    cancellationToken);

                decimal saldoInicial = 0;
                foreach (var t in transacoesAnteriores)
                {
                    if (t.Tipo == Domain.Enums.TipoOperacao.Credito || t.Tipo == Domain.Enums.TipoOperacao.Estorno)
                        saldoInicial += t.Valor;
                    else if (t.Tipo == Domain.Enums.TipoOperacao.Debito || t.Tipo == Domain.Enums.TipoOperacao.Captura)
                        saldoInicial -= t.Valor;
                }

                var extratoDTO = new ExtratoDTO
                {
                    ContaId = conta.Id,
                    NumeroConta = conta.Numero,
                    DataInicio = request.DataInicio,
                    DataFim = request.DataFim,
                    SaldoInicial = saldoInicial,
                    SaldoFinal = conta.SaldoDisponivel,
                    Transacoes = transacoesDTO,
                    TotalTransacoes = transacoesDTO.Count
                };

                _logger.LogInformation("Extrato gerado com sucesso - Conta: {ContaId}, Total transações: {Total}", request.ContaId, transacoesDTO.Count);

                return OperationResult<ExtratoDTO>.SuccessResult(extratoDTO, "Extrato gerado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar extrato da conta: {ContaId}", request.ContaId);
                return OperationResult<ExtratoDTO>.FailureResult("Erro ao gerar extrato", ex.Message);
            }
        }
    }
}