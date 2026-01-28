using MediatR;
using Microsoft.AspNetCore.Mvc;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.Queries;

namespace SL.DesafioPagueVeloz.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ContasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ContasController> _logger;

        public ContasController(IMediator mediator, ILogger<ContasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Cria uma nova conta para um cliente
        /// </summary>
        /// <param name="command">Dados da conta</param>
        /// <returns>Conta criada</returns>
        /// <response code="200">Conta criada com sucesso</response>
        /// <response code="400">Dados inválidos ou cliente não existe</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarConta([FromBody] CriarContaCommand command)
        {
            _logger.LogInformation("Requisição para criar conta: {Numero} para cliente: {ClienteId}",
                command.Numero, command.ClienteId);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao criar conta: {Message}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Conta criada com sucesso: {ContaId}", result.Data?.Id);
            return Ok(result);
        }

        /// <summary>
        /// Obtém uma conta por ID
        /// </summary>
        /// <param name="id">ID da conta</param>
        /// <returns>Dados da conta</returns>
        /// <response code="200">Conta encontrada</response>
        /// <response code="404">Conta não encontrada</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterContaPorId(Guid id)
        {
            _logger.LogInformation("Requisição para obter conta: {ContaId}", id);

            var query = new ObterContaPorIdQuery { ContaId = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                _logger.LogWarning("Conta não encontrada: {ContaId}", id);
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtém o saldo de uma conta
        /// </summary>
        /// <param name="id">ID da conta</param>
        /// <returns>Saldo da conta</returns>
        /// <response code="200">Saldo obtido com sucesso</response>
        /// <response code="404">Conta não encontrada</response>
        [HttpGet("{id:guid}/saldo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterSaldo(Guid id)
        {
            _logger.LogInformation("Requisição para obter saldo da conta: {ContaId}", id);

            var query = new ObterSaldoQuery { ContaId = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                _logger.LogWarning("Conta não encontrada para consulta de saldo: {ContaId}", id);
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtém o extrato de uma conta por período
        /// </summary>
        /// <param name="id">ID da conta</param>
        /// <param name="dataInicio">Data início (formato: yyyy-MM-dd)</param>
        /// <param name="dataFim">Data fim (formato: yyyy-MM-dd)</param>
        /// <returns>Extrato da conta</returns>
        /// <response code="200">Extrato gerado com sucesso</response>
        /// <response code="400">Datas inválidas</response>
        /// <response code="404">Conta não encontrada</response>
        [HttpGet("{id:guid}/extrato")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterExtrato(
            Guid id,
            [FromQuery] DateTime dataInicio,
            [FromQuery] DateTime dataFim)
        {
            _logger.LogInformation("Requisição para obter extrato da conta: {ContaId} de {DataInicio} até {DataFim}",
                id, dataInicio, dataFim);

            if (dataInicio > dataFim)
            {
                return BadRequest(new { message = "Data início não pode ser maior que data fim" });
            }

            var query = new ObterExtratoQuery
            {
                ContaId = id,
                DataInicio = dataInicio,
                DataFim = dataFim
            };

            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao gerar extrato: {Message}", result.Message);

                if (result.Message.Contains("não encontrada"))
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Lista todas as transações de uma conta
        /// </summary>
        /// <param name="id">ID da conta</param>
        /// <returns>Lista de transações</returns>
        /// <response code="200">Transações listadas com sucesso</response>
        /// <response code="404">Conta não encontrada</response>
        [HttpGet("{id:guid}/transacoes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListarTransacoes(Guid id)
        {
            _logger.LogInformation("Requisição para listar transações da conta: {ContaId}", id);

            var query = new ListarTransacoesQuery { ContaId = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                _logger.LogWarning("Conta não encontrada para listagem de transações: {ContaId}", id);
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}