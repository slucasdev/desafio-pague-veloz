using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Queries;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class ObterClientePorIdQueryHandler : IRequestHandler<ObterClientePorIdQuery, OperationResult<ClienteDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObterClientePorIdQueryHandler> _logger;
        private readonly IMapper _mapper;

        public ObterClientePorIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<ObterClientePorIdQueryHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<ClienteDTO>> Handle(
            ObterClientePorIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Buscando cliente: {ClienteId}", request.ClienteId);

                var cliente = await _unitOfWork.Clientes.ObterPorIdAsync(request.ClienteId, cancellationToken);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente não encontrado: {ClienteId}", request.ClienteId);
                    return OperationResult<ClienteDTO>.FailureResult("Cliente não encontrado", "ClienteId inválido");
                }

                _logger.LogInformation("Cliente encontrado: {ClienteId}", request.ClienteId);

                return OperationResult<ClienteDTO>.SuccessResult(_mapper.Map<ClienteDTO>(cliente), "Cliente encontrado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar cliente: {ClienteId}", request.ClienteId);
                return OperationResult<ClienteDTO>.FailureResult("Erro ao buscar cliente", ex.Message);
            }
        }
    }
}