using FluentAssertions;
using SL.DesafioPagueVeloz.Api.Tests.Fixtures;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using System.Net;
using System.Net.Http.Json;

namespace SL.DesafioPagueVeloz.Api.Tests.Integration
{
    public class IdempotenciaTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public IdempotenciaTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Creditar_ComMesmaIdempotencyKey_DeveRetornarMesmaTransacao()
        {
            // Arrange
            var contaId = await CriarClienteEConta();
            var idempotencyKey = Guid.NewGuid();

            var command = new CreditarContaCommand
            {
                ContaId = contaId,
                Valor = 500m,
                Descricao = "Depósito teste idempotência",
                IdempotencyKey = idempotencyKey
            };

            // Act - Fazer 3 chamadas com MESMA idempotencyKey
            var response1 = await _client.PostAsJsonAsync("/api/transacoes/creditar", command);
            var response2 = await _client.PostAsJsonAsync("/api/transacoes/creditar", command);
            var response3 = await _client.PostAsJsonAsync("/api/transacoes/creditar", command);

            var result1 = await response1.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();
            var result2 = await response2.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();
            var result3 = await response3.Content.ReadFromJsonAsync<OperationResult<TransacaoDTO>>();

            // Assert
            result1!.Success.Should().BeTrue();
            result2!.Success.Should().BeTrue();
            result3!.Success.Should().BeTrue();

            // MESMA transação retornada nas 3 chamadas
            result1.Data!.Id.Should().Be(result2.Data!.Id);
            result2.Data!.Id.Should().Be(result3.Data!.Id);

            // Verificar saldo (deve ter creditado APENAS UMA VEZ)
            var responseSaldo = await _client.GetAsync($"/api/contas/{contaId}/saldo");
            var resultSaldo = await responseSaldo.Content.ReadFromJsonAsync<OperationResult<SaldoDTO>>();

            resultSaldo!.Data!.SaldoDisponivel.Should().Be(500m); // Apenas 1 crédito
        }

        [Fact]
        public async Task Transferir_ComMesmaIdempotencyKey_DeveExecutarApenasUmaVez()
        {
            // Arrange
            var contaOrigemId = await CriarClienteEConta();
            var contaDestinoId = await CriarClienteEConta();

            await CreditarConta(contaOrigemId, 1000m);

            var idempotencyKey = Guid.NewGuid();
            var command = new TransferirCommand
            {
                ContaOrigemId = contaOrigemId,
                ContaDestinoId = contaDestinoId,
                Valor = 300m,
                Descricao = "Transferência idempotência",
                IdempotencyKey = idempotencyKey
            };

            // Act - Fazer 2 chamadas com MESMA idempotencyKey
            var response1 = await _client.PostAsJsonAsync("/api/transacoes/transferir", command);
            var response2 = await _client.PostAsJsonAsync("/api/transacoes/transferir", command);

            var result1 = await response1.Content.ReadFromJsonAsync<OperationResult<List<TransacaoDTO>>>();
            var result2 = await response2.Content.ReadFromJsonAsync<OperationResult<List<TransacaoDTO>>>();

            // Assert
            result1!.Success.Should().BeTrue();
            result2!.Success.Should().BeTrue();

            // Verificar saldos (transferência executada APENAS UMA VEZ)
            var responseSaldoOrigem = await _client.GetAsync($"/api/contas/{contaOrigemId}/saldo");
            var resultSaldoOrigem = await responseSaldoOrigem.Content.ReadFromJsonAsync<OperationResult<SaldoDTO>>();
            resultSaldoOrigem!.Data!.SaldoDisponivel.Should().Be(700m); // 1000 - 300

            var responseSaldoDestino = await _client.GetAsync($"/api/contas/{contaDestinoId}/saldo");
            var resultSaldoDestino = await responseSaldoDestino.Content.ReadFromJsonAsync<OperationResult<SaldoDTO>>();
            resultSaldoDestino!.Data!.SaldoDisponivel.Should().Be(300m); // Apenas 1 crédito
        }

        #region Helpers

        private async Task<Guid> CriarClienteEConta()
        {
            var commandCliente = new CriarClienteCommand
            {
                Nome = $"Cliente {Guid.NewGuid()}",
                Documento = DocumentoHelper.GerarCPFValido(),
                Email = $"teste{Guid.NewGuid()}@email.com"
            };

            var responseCliente = await _client.PostAsJsonAsync("/api/clientes", commandCliente);
            var resultCliente = await responseCliente.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();
            var clienteId = resultCliente!.Data!.Id;

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

        #endregion
    }
}