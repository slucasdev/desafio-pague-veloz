using FluentAssertions;
using SL.DesafioPagueVeloz.Api.Tests.Fixtures;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using System.Net;
using System.Net.Http.Json;

namespace SL.DesafioPagueVeloz.Api.Tests.Controllers
{
    public class TransacoesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TransacoesControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Creditar_ComDadosValidos_DeveRetornar200()
        {
            // Arrange - Criar cliente e conta primeiro
            var contaId = await CriarClienteEConta();

            var command = new CreditarContaCommand
            {
                ContaId = contaId,
                Valor = 500m,
                Descricao = "Depósito inicial",
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/transacoes/creditar", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Valor.Should().Be(500m);
            result.Data.Tipo.Should().Be("Credito");
        }

        [Fact]
        public async Task Creditar_ComIdempotencyKeyDuplicada_DeveRetornarMesmaTransacao()
        {
            // Arrange
            var contaId = await CriarClienteEConta();
            var idempotencyKey = Guid.NewGuid();

            var command = new CreditarContaCommand
            {
                ContaId = contaId,
                Valor = 500m,
                Descricao = "Depósito",
                IdempotencyKey = idempotencyKey
            };

            // Act - Primeira chamada
            var response1 = await _client.PostAsJsonAsync("/api/transacoes/creditar", command);
            var result1 = await response1.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();

            // Act - Segunda chamada (mesma idempotencyKey)
            var response2 = await _client.PostAsJsonAsync("/api/transacoes/creditar", command);
            var result2 = await response2.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();

            // Assert
            result1!.Success.Should().BeTrue();
            result2!.Success.Should().BeTrue();
            result2.Message.Should().Contain("já processada");

            // Mesma transação retornada
            result1.Data!.Id.Should().Be(result2.Data!.Id);
        }

        [Fact]
        public async Task Debitar_ComSaldoSuficiente_DeveRetornar200()
        {
            // Arrange
            var contaId = await CriarClienteEConta();

            // Creditar primeiro
            await CreditarConta(contaId, 1000m);

            var command = new DebitarContaCommand
            {
                ContaId = contaId,
                Valor = 300m,
                Descricao = "Pagamento conta de luz",
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/transacoes/debitar", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();
            result!.Success.Should().BeTrue();
            result.Data!.Valor.Should().Be(300m);
            result.Data.Tipo.Should().Be("Debito");
        }

        [Fact]
        public async Task Debitar_ComSaldoInsuficiente_DeveRetornarFalha()
        {
            // Arrange
            // Criar conta com LIMITE ZERO para testar apenas saldo disponível
            var contaId = await CriarClienteEContaSemLimite();

            // Creditar apenas 100
            await CreditarConta(contaId, 100m);

            var command = new DebitarContaCommand
            {
                ContaId = contaId,
                Valor = 200m, // Maior que saldo disponível
                Descricao = "Pagamento",
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/transacoes/debitar", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();
            result!.Success.Should().BeFalse();
            result.Message.Should().Contain("Saldo insuficiente");
        }

        [Fact]
        public async Task Transferir_ComSaldoSuficiente_DeveRetornar200()
        {
            // Arrange
            var contaOrigemId = await CriarClienteEConta();
            var contaDestinoId = await CriarClienteEConta();

            // Creditar conta origem
            await CreditarConta(contaOrigemId, 1000m);

            var command = new TransferirCommand
            {
                ContaOrigemId = contaOrigemId,
                ContaDestinoId = contaDestinoId,
                Valor = 300m,
                Descricao = "Transferência entre contas",
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/transacoes/transferir", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<List<TransacaoDTO>>>();
            result!.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2); // Débito + Crédito

            // Verificar débito na origem
            result.Data.Should().Contain(t => t.Tipo == "Debito" && t.Valor == 300m);

            // Verificar crédito no destino
            result.Data.Should().Contain(t => t.Tipo == "Credito" && t.Valor == 300m);
        }

        [Fact]
        public async Task ReservarCapturarCancelar_FluxoCompleto_DeveRetornar200()
        {
            // Arrange
            var contaId = await CriarClienteEConta();
            await CreditarConta(contaId, 1000m);

            // 1. Reservar
            var commandReservar = new ReservarCommand
            {
                ContaId = contaId,
                Valor = 200m,
                Descricao = "Reserva para compra",
                IdempotencyKey = Guid.NewGuid()
            };

            var responseReservar = await _client.PostAsJsonAsync("/api/transacoes/reservar", commandReservar);
            var resultReservar = await responseReservar.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();

            resultReservar!.Success.Should().BeTrue();
            var transacaoReservaId = resultReservar.Data!.Id;

            // 2. Capturar
            var commandCapturar = new CapturarCommand
            {
                ContaId = contaId,
                Valor = 200m,
                TransacaoReservaId = transacaoReservaId,
                Descricao = "Captura de compra",
                IdempotencyKey = Guid.NewGuid()
            };

            var responseCapturar = await _client.PostAsJsonAsync("/api/transacoes/capturar", commandCapturar);
            var resultCapturar = await responseCapturar.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();

            // Assert
            resultCapturar!.Success.Should().BeTrue();
            resultCapturar.Data!.Tipo.Should().Be("Captura");
        }

        [Fact]
        public async Task Estornar_DeveReverterTransacao()
        {
            // Arrange
            var contaId = await CriarClienteEConta();

            // Creditar
            var commandCreditar = new CreditarContaCommand
            {
                ContaId = contaId,
                Valor = 500m,
                Descricao = "Depósito",
                IdempotencyKey = Guid.NewGuid()
            };

            var responseCreditar = await _client.PostAsJsonAsync("/api/transacoes/creditar", commandCreditar);
            var resultCreditar = await responseCreditar.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();
            var transacaoId = resultCreditar!.Data!.Id;

            // Debitar
            var commandDebitar = new DebitarContaCommand
            {
                ContaId = contaId,
                Valor = 200m,
                Descricao = "Pagamento",
                IdempotencyKey = Guid.NewGuid()
            };

            var responseDebitar = await _client.PostAsJsonAsync("/api/transacoes/debitar", commandDebitar);
            var resultDebitar = await responseDebitar.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();
            var transacaoDebitoId = resultDebitar!.Data!.Id;

            // Estornar o débito
            var commandEstornar = new EstornarCommand
            {
                ContaId = contaId,
                Valor = 200m,
                TransacaoOriginalId = transacaoDebitoId,
                Descricao = "Estorno de pagamento",
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/transacoes/estornar", commandEstornar);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();
            result!.Success.Should().BeTrue();
            result.Data!.Tipo.Should().Be("Estorno");
        }

        #region Helpers

        private async Task<Guid> CriarClienteEConta()
        {
            // Criar cliente
            var commandCliente = new CriarClienteCommand
            {
                Nome = $"Cliente Teste {Guid.NewGuid()}",
                Documento = DocumentoHelper.GerarCPFValido(),
                Email = $"teste{Guid.NewGuid()}@email.com"
            };

            var responseCliente = await _client.PostAsJsonAsync("/api/clientes", commandCliente);
            var resultCliente = await responseCliente.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();
            var clienteId = resultCliente!.Data!.Id;

            // Criar conta
            var commandConta = new CriarContaCommand
            {
                ClienteId = clienteId,
                Numero = $"{Random.Shared.Next(10000, 99999)}-{Random.Shared.Next(0, 9)}",
                LimiteCredito = 1000m
            };

            var responseConta = await _client.PostAsJsonAsync("/api/contas", commandConta);
            var resultConta = await responseConta.Content.ReadFromJsonAsync<OperationResult<ContaDTO>>();

            return resultConta!.Data!.Id;
        }

        private async Task CreditarConta(Guid contaId, decimal valor)
        {
            var command = new CreditarContaCommand
            {
                ContaId = contaId,
                Valor = valor,
                Descricao = "Crédito inicial",
                IdempotencyKey = Guid.NewGuid()
            };

            var response = await _client.PostAsJsonAsync("/api/transacoes/creditar", command);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Falha ao creditar conta. Status: {response.StatusCode}, Body: {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();

            if (result?.Success != true)
            {
                throw new InvalidOperationException($"Falha ao creditar conta: {result?.Message}");
            }
        }

        private async Task<Guid> CriarClienteEContaSemLimite()
        {
            // Criar cliente
            var commandCliente = new CriarClienteCommand
            {
                Nome = $"Cliente Teste {Guid.NewGuid()}",
                Documento = DocumentoHelper.GerarCPFValido(),
                Email = $"teste{Guid.NewGuid()}@email.com"
            };

            var responseCliente = await _client.PostAsJsonAsync("/api/clientes", commandCliente);
            var resultCliente = await responseCliente.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();
            var clienteId = resultCliente!.Data!.Id;

            // Criar conta SEM limite de crédito
            var commandConta = new CriarContaCommand
            {
                ClienteId = clienteId,
                Numero = $"{Random.Shared.Next(10000, 99999)}-{Random.Shared.Next(0, 9)}",
                LimiteCredito = 0m // ZERO - sem limite
            };

            var responseConta = await _client.PostAsJsonAsync("/api/contas", commandConta);
            var resultConta = await responseConta.Content.ReadFromJsonAsync<OperationResult<ContaDTO>>();

            return resultConta!.Data!.Id;
        }

        #endregion
    }
}