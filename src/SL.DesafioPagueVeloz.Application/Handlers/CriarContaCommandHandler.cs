using AutoMapper;
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
        private readonly IMapper _mapper;

        public CriarContaCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CriarContaCommandHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<ContaDTO>> Handle(
            CriarContaCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de conta: {Numero} para cliente: {ClienteId}", request.Numero, request.ClienteId);

                var cliente = await _unitOfWork.Clientes.ObterPorIdAsync(request.ClienteId, cancellationToken);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente não encontrado: {ClienteId}", request.ClienteId);
                    return OperationResult<ContaDTO>.FailureResult("Cliente não encontrado", "ClienteId inválido");
                }

                var numeroExiste = await _unitOfWork.Contas.ExisteNumeroAsync(request.Numero, cancellationToken);

                if (numeroExiste)
                {
                    _logger.LogWarning("Tentativa de criar conta com número duplicado: {Numero}", request.Numero);
                    return OperationResult<ContaDTO>.FailureResult("Número de conta já existe", "Número duplicado");
                }

                var conta = Conta.Criar(request.ClienteId, request.Numero, request.LimiteCredito);

                await _unitOfWork.Contas.AdicionarAsync(conta, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Conta criada com sucesso: {ContaId}", conta.Id);

                return OperationResult<ContaDTO>.SuccessResult(_mapper.Map<ContaDTO>(conta), "Conta criada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar conta: {Numero}", request.Numero);
                return OperationResult<ContaDTO>.FailureResult("Erro ao criar conta", ex.Message);
            }
        }
    }
}