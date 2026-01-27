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

        public ObterClientePorIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<ObterClientePorIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                    return OperationResult<ClienteDTO>.FailureResult(
                        "Cliente não encontrado",
                        "ClienteId inválido");
                }

                var clienteDTO = new ClienteDTO
                {
                    Id = cliente.Id,
                    Nome = cliente.Nome,
                    Documento = cliente.Documento.Numero,
                    TipoDocumento = cliente.Documento.Tipo.ToString(),
                    Email = cliente.Email,
                    Ativo = cliente.Ativo,
                    CriadoEm = cliente.CriadoEm,
                    AtualizadoEm = cliente.AtualizadoEm
                };

                _logger.LogInformation("Cliente encontrado: {ClienteId}", request.ClienteId);

                return OperationResult<ClienteDTO>.SuccessResult(clienteDTO, "Cliente encontrado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar cliente: {ClienteId}", request.ClienteId);
                return OperationResult<ClienteDTO>.FailureResult(
                    "Erro ao buscar cliente",
                    ex.Message);
            }
        }
    }
}
