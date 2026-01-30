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
    public class CriarClienteCommandHandler : IRequestHandler<CriarClienteCommand, OperationResult<ClienteDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CriarClienteCommandHandler> _logger;
        private readonly IMapper _mapper;

        public CriarClienteCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CriarClienteCommandHandler> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<ClienteDTO>> Handle(
            CriarClienteCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de cliente: {Nome}", request.Nome);

                var documentoExiste = await _unitOfWork.Clientes.ExisteDocumentoAsync(request.Documento, cancellationToken);

                if (documentoExiste)
                {
                    _logger.LogWarning("Tentativa de criar cliente com documento duplicado: {Documento}", request.Documento);
                    return OperationResult<ClienteDTO>.FailureResult("Cliente já cadastrado com este documento", "Documento duplicado");
                }

                var cliente = Cliente.Criar(request.Nome, request.Documento, request.Email);

                await _unitOfWork.Clientes.AdicionarAsync(cliente, cancellationToken);

                _logger.LogInformation("Cliente criado com sucesso: {ClienteId}", cliente.Id);

                return OperationResult<ClienteDTO>.SuccessResult(_mapper.Map<ClienteDTO>(cliente), "Cliente criado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cliente: {Nome}", request.Nome);
                return OperationResult<ClienteDTO>.FailureResult("Erro ao criar cliente", ex.Message);
            }
        }
    }
}