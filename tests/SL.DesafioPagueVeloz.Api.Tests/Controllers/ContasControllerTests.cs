using FluentAssertions;
using SL.DesafioPagueVeloz.Api.Tests.Fixtures;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using System.Net;
using System.Net.Http.Json;

namespace SL.DesafioPagueVeloz.Api.Tests.Controllers
{
    public class ContasControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ContasControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CriarConta_ComDadosValidos_DeveRetornar200()
        {
            // Arrange - Criar cliente primeiro
            var clienteId = await CriarCliente();

            var command = new CriarContaCommand
            {
                ClienteId = clienteId,
                Numero = "00001-5",
                LimiteCredito = 1000m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/contas", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<ContaDTO>>();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Numero.Should().Be("00001-5");
            result.Data.LimiteCredito.Should().Be(1000m);
            result.Data.SaldoDisponivel.Should().Be(0);
        }

        [Fact]
        public async Task ObterSaldo_ComContaExistente_DeveRetornar200()
        {
            // Arrange
            var clienteId = await CriarCliente();
            var contaId = await CriarConta(clienteId);

            // Act
            var response = await _client.GetAsync($"/api/contas/{contaId}/saldo");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<SaldoDTO>>();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.SaldoDisponivel.Should().Be(0);
        }

        [Fact]
        public async Task ObterExtrato_ComPeriodoValido_DeveRetornar200()
        {
            // Arrange
            var clienteId = await CriarCliente();
            var contaId = await CriarConta(clienteId);

            // Fazer algumas transações
            await CreditarConta(contaId, 500m);
            await DebitarConta(contaId, 100m);

            var dataInicio = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            var dataFim = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

            // Act
            var response = await _client.GetAsync($"/api/contas/{contaId}/extrato?dataInicio={dataInicio}&dataFim={dataFim}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<ExtratoDTO>>();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Transacoes.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task ListarTransacoes_ComContaComTransacoes_DeveRetornar200()
        {
            // Arrange
            var clienteId = await CriarCliente();
            var contaId = await CriarConta(clienteId);

            await CreditarConta(contaId, 500m);
            await DebitarConta(contaId, 100m);

            // Act
            var response = await _client.GetAsync($"/api/contas/{contaId}/transacoes");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<List<TransacaoDTO>>>();
            result!.Success.Should().BeTrue();
            result.Data.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        #region Helpers

        private async Task<Guid> CriarCliente()
        {
            var command = new CriarClienteCommand
            {
                Nome = $"Cliente {Guid.NewGuid()}",
                Documento = DocumentoHelper.GerarCPFValido(),
                Email = $"cliente{Guid.NewGuid()}@email.com"
            };

            var response = await _client.PostAsJsonAsync("/api/clientes", command);
            var result = await response.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();

            return result!.Data!.Id;
        }

        private async Task<Guid> CriarConta(Guid clienteId)
        {
            var command = new CriarContaCommand
            {
                ClienteId = clienteId,
                Numero = $"{Random.Shared.Next(10000, 99999)}-{Random.Shared.Next(0, 9)}",
                LimiteCredito = 1000m
            };

            var response = await _client.PostAsJsonAsync("/api/contas", command);
            var result = await response.Content.ReadFromJsonAsync<OperationResult<ContaDTO>>();

            return result!.Data!.Id;
        }

        private async Task CreditarConta(Guid contaId, decimal valor)
        {
            var command = new CreditarContaCommand
            {
                ContaId = contaId,
                Valor = valor,
                Descricao = "Crédito",
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

        private async Task DebitarConta(Guid contaId, decimal valor)
        {
            var command = new DebitarContaCommand
            {
                ContaId = contaId,
                Valor = valor,
                Descricao = "Débito",
                IdempotencyKey = Guid.NewGuid()
            };

            var response = await _client.PostAsJsonAsync("/api/transacoes/debitar", command);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Falha ao debitar conta. Status: {response.StatusCode}, Body: {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();

            if (result?.Success != true)
            {
                throw new InvalidOperationException($"Falha ao debitar conta: {result?.Message}");
            }
        }

        #endregion
    }
}