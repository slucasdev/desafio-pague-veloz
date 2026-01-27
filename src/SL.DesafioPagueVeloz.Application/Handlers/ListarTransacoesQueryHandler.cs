using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Queries;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class ListarTransacoesQueryHandler : IRequestHandler<ListarTransacoesQuery, OperationResult<List<TransacaoDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListarTransacoesQueryHandler> _logger;

        public ListarTransacoesQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<ListarTransacoesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OperationResult<List<TransacaoDTO>>> Handle(
            ListarTransacoesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Listando transações da conta: {ContaId}", request.ContaId);

                var conta = await _unitOfWork.Contas.ObterPorIdAsync(request.ContaId, cancellationToken);

                if (conta == null)
                {
                    _logger.LogWarning("Conta não encontrada: {ContaId}", request.ContaId);
                    return OperationResult<List<TransacaoDTO>>.FailureResult(
                        "Conta não encontrada",
                        "ContaId inválido");
                }

                var transacoes = await _unitOfWork.Transacoes.ObterPorContaIdAsync(request.ContaId, cancellationToken);

                var transacoesDTO = transacoes.Select(t => new TransacaoDTO
                {
                    Id = t.Id,
                    ContaId = t.ContaId,
                    Tipo = t.Tipo.ToString(),
                    Valor = t.Valor,
                    Descricao = t.Descricao,
                    Status = t.Status.ToString(),
                    IdempotencyKey = t.IdempotencyKey,
                    TransacaoOrigemId = t.TransacaoOrigemId,
                    ProcessadoEm = t.ProcessadoEm,
                    MotivoFalha = t.MotivoFalha,
                    CriadoEm = t.CriadoEm
                }).ToList();

                _logger.LogInformation("Transações listadas com sucesso - Conta: {ContaId}, Total: {Total}",
                    request.ContaId, transacoesDTO.Count);

                return OperationResult<List<TransacaoDTO>>.SuccessResult(transacoesDTO, "Transações listadas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar transações da conta: {ContaId}", request.ContaId);
                return OperationResult<List<TransacaoDTO>>.FailureResult(
                    "Erro ao listar transações",
                    ex.Message);
            }
        }
    }
}
