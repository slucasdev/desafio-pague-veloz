using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Handlers
{
    public class CriarClienteCommandHandler : IRequestHandler<CriarClienteCommand, OperationResult<ClienteDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CriarClienteCommandHandler> _logger;

        public CriarClienteCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CriarClienteCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OperationResult<ClienteDTO>> Handle(
            CriarClienteCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de cliente: {Nome}", request.Nome);

                // Verificar se documento já existe
                var documentoExiste = await _unitOfWork.Clientes
                    .ExisteDocumentoAsync(request.Documento, cancellationToken);

                if (documentoExiste)
                {
                    _logger.LogWarning("Tentativa de criar cliente com documento duplicado: {Documento}", request.Documento);
                    return OperationResult<ClienteDTO>.FailureResult(
                        "Cliente já cadastrado com este documento",
                        "Documento duplicado");
                }

                // Criar cliente
                var cliente = Cliente.Criar(request.Nome, request.Documento, request.Email);

                await _unitOfWork.Clientes.AdicionarAsync(cliente, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Cliente criado com sucesso: {ClienteId}", cliente.Id);

                // Mapear para DTO
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

                return OperationResult<ClienteDTO>.SuccessResult(clienteDTO, "Cliente criado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cliente: {Nome}", request.Nome);
                return OperationResult<ClienteDTO>.FailureResult(
                    "Erro ao criar cliente",
                    ex.Message);
            }
        }
    }
}
