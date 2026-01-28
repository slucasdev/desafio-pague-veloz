using MediatR;
using Microsoft.AspNetCore.Mvc;
using SL.DesafioPagueVeloz.Application.Commands;

namespace SL.DesafioPagueVeloz.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransacoesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransacoesController> _logger;

        public TransacoesController(IMediator mediator, ILogger<TransacoesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Realiza um crédito em uma conta
        /// </summary>
        /// <param name="command">Dados da operação de crédito</param>
        /// <returns>Transação criada</returns>
        /// <response code="200">Crédito realizado com sucesso</response>
        /// <response code="400">Dados inválidos ou conta não encontrada</response>
        [HttpPost("creditar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Creditar([FromBody] CreditarContaCommand command)
        {
            _logger.LogInformation("Requisição de crédito - Conta: {ContaId}, Valor: {Valor}",
                command.ContaId, command.Valor);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao creditar: {Message}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Crédito realizado com sucesso - Transação: {TransacaoId}",
                result.Data?.Id);

            return Ok(result);
        }

        /// <summary>
        /// Realiza um débito em uma conta
        /// </summary>
        /// <param name="command">Dados da operação de débito</param>
        /// <returns>Transação criada</returns>
        /// <response code="200">Débito realizado com sucesso</response>
        /// <response code="400">Saldo insuficiente, conta bloqueada ou dados inválidos</response>
        [HttpPost("debitar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Debitar([FromBody] DebitarContaCommand command)
        {
            _logger.LogInformation("Requisição de débito - Conta: {ContaId}, Valor: {Valor}",
                command.ContaId, command.Valor);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao debitar: {Message}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Débito realizado com sucesso - Transação: {TransacaoId}",
                result.Data?.Id);

            return Ok(result);
        }

        /// <summary>
        /// Realiza uma reserva de valor em uma conta
        /// </summary>
        /// <param name="command">Dados da operação de reserva</param>
        /// <returns>Transação criada</returns>
        /// <response code="200">Reserva realizada com sucesso</response>
        /// <response code="400">Saldo insuficiente ou dados inválidos</response>
        [HttpPost("reservar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reservar([FromBody] ReservarCommand command)
        {
            _logger.LogInformation("Requisição de reserva - Conta: {ContaId}, Valor: {Valor}",
                command.ContaId, command.Valor);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao reservar: {Message}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Reserva realizada com sucesso - Transação: {TransacaoId}",
                result.Data?.Id);

            return Ok(result);
        }

        /// <summary>
        /// Captura um valor previamente reservado
        /// </summary>
        /// <param name="command">Dados da operação de captura</param>
        /// <returns>Transação criada</returns>
        /// <response code="200">Captura realizada com sucesso</response>
        /// <response code="400">Reserva não encontrada ou dados inválidos</response>
        [HttpPost("capturar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Capturar([FromBody] CapturarCommand command)
        {
            _logger.LogInformation("Requisição de captura - Conta: {ContaId}, Valor: {Valor}, Reserva: {ReservaId}",
                command.ContaId, command.Valor, command.TransacaoReservaId);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao capturar: {Message}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Captura realizada com sucesso - Transação: {TransacaoId}",
                result.Data?.Id);

            return Ok(result);
        }

        /// <summary>
        /// Cancela uma reserva previamente realizada
        /// </summary>
        /// <param name="command">Dados da operação de cancelamento</param>
        /// <returns>Transação criada</returns>
        /// <response code="200">Cancelamento realizado com sucesso</response>
        /// <response code="400">Reserva não encontrada ou dados inválidos</response>
        [HttpPost("cancelar-reserva")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelarReserva([FromBody] CancelarReservaCommand command)
        {
            _logger.LogInformation("Requisição de cancelamento de reserva - Conta: {ContaId}, Reserva: {ReservaId}",
                command.ContaId, command.TransacaoReservaId);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao cancelar reserva: {Message}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Cancelamento de reserva realizado com sucesso - Transação: {TransacaoId}",
                result.Data?.Id);

            return Ok(result);
        }

        /// <summary>
        /// Estorna uma transação previamente realizada
        /// </summary>
        /// <param name="command">Dados da operação de estorno</param>
        /// <returns>Transação criada</returns>
        /// <response code="200">Estorno realizado com sucesso</response>
        /// <response code="400">Transação não encontrada ou dados inválidos</response>
        [HttpPost("estornar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Estornar([FromBody] EstornarCommand command)
        {
            _logger.LogInformation("Requisição de estorno - Conta: {ContaId}, Transação Original: {TransacaoId}",
                command.ContaId, command.TransacaoOriginalId);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao estornar: {Message}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Estorno realizado com sucesso - Transação: {TransacaoId}",
                result.Data?.Id);

            return Ok(result);
        }

        /// <summary>
        /// Realiza uma transferência entre contas
        /// </summary>
        /// <param name="command">Dados da operação de transferência</param>
        /// <returns>Transações criadas (débito e crédito)</returns>
        /// <response code="200">Transferência realizada com sucesso</response>
        /// <response code="400">Saldo insuficiente, conta bloqueada ou dados inválidos</response>
        [HttpPost("transferir")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Transferir([FromBody] TransferirCommand command)
        {
            _logger.LogInformation("Requisição de transferência - Origem: {Origem}, Destino: {Destino}, Valor: {Valor}",
                command.ContaOrigemId, command.ContaDestinoId, command.Valor);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao transferir: {Message}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Transferência realizada com sucesso");

            return Ok(result);
        }
    }
}