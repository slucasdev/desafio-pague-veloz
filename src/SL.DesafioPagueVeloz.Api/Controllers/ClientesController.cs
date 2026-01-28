using MediatR;
using Microsoft.AspNetCore.Mvc;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.Queries;

namespace SL.DesafioPagueVeloz.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(IMediator mediator, ILogger<ClientesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo cliente
        /// </summary>
        /// <param name="command">Dados do cliente</param>
        /// <returns>Cliente criado</returns>
        /// <response code="200">Cliente criado com sucesso</response>
        /// <response code="400">Dados inválidos ou cliente já existe</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarCliente([FromBody] CriarClienteCommand command)
        {
            _logger.LogInformation("Requisição para criar cliente: {Nome}", command.Nome);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao criar cliente: {Message}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Cliente criado com sucesso: {ClienteId}", result.Data?.Id);
            return Ok(result);
        }

        /// <summary>
        /// Obtém um cliente por ID
        /// </summary>
        /// <param name="id">ID do cliente</param>
        /// <returns>Dados do cliente</returns>
        /// <response code="200">Cliente encontrado</response>
        /// <response code="404">Cliente não encontrado</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterClientePorId(Guid id)
        {
            _logger.LogInformation("Requisição para obter cliente: {ClienteId}", id);

            var query = new ObterClientePorIdQuery { ClienteId = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                _logger.LogWarning("Cliente não encontrado: {ClienteId}", id);
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}