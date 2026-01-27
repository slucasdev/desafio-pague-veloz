using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class CriarContaCommandHandler : IRequestHandler<CriarContaCommand, OperationResult<ContaDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CriarContaCommandHandler> _logger;

        public CriarContaCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CriarContaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OperationResult<ContaDTO>> Handle(
            CriarContaCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de conta: {Numero} para cliente: {ClienteId}",
                    request.Numero, request.ClienteId);

                // Verificar se cliente existe
                var cliente = await _unitOfWork.Clientes
                    .ObterPorIdAsync(request.ClienteId, cancellationToken);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente não encontrado: {ClienteId}", request.ClienteId);
                    return OperationResult<ContaDTO>.FailureResult(
                        "Cliente não encontrado",
                        "ClienteId inválido");
                }

                // Verificar se número da conta já existe
                var numeroExiste = await _unitOfWork.Contas
                    .ExisteNumeroAsync(request.Numero, cancellationToken);

                if (numeroExiste)
                {
                    _logger.LogWarning("Tentativa de criar conta com número duplicado: {Numero}", request.Numero);
                    return OperationResult<ContaDTO>.FailureResult(
                        "Número de conta já existe",
                        "Número duplicado");
                }

                // Criar conta
                var conta = Conta.Criar(request.ClienteId, request.Numero, request.LimiteCredito);

                await _unitOfWork.Contas.AdicionarAsync(conta, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Conta criada com sucesso: {ContaId}", conta.Id);

                // Mapear para DTO
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

                return OperationResult<ContaDTO>.SuccessResult(contaDTO, "Conta criada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar conta: {Numero}", request.Numero);
                return OperationResult<ContaDTO>.FailureResult(
                    "Erro ao criar conta",
                    ex.Message);
            }
        }
    }
}
