using FluentAssertions;
using SL.DesafioPagueVeloz.Api.Tests.Fixtures;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;
using System.Net;
using System.Net.Http.Json;

namespace SL.DesafioPagueVeloz.Api.Tests.Controllers
{
    public class ClientesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ClientesControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CriarCliente_ComDadosValidos_DeveRetornar200()
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = "João Silva",
                Documento = DocumentoHelper.GerarCPFValido(),
                Email = $"joao{Guid.NewGuid()}@email.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/clientes", command);

            // Debug
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Error: {errorBody}");
            }

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Nome.Should().Be("João Silva");
            result.Data.Ativo.Should().BeTrue();
        }

        [Fact]
        public async Task CriarCliente_ComDocumentoDuplicado_DeveRetornar400()
        {
            // Arrange
            var documento = DocumentoHelper.GerarCPFValido();

            var command1 = new CriarClienteCommand
            {
                Nome = "Cliente 1",
                Documento = documento,
                Email = "cliente1@email.com"
            };

            var command2 = new CriarClienteCommand
            {
                Nome = "Cliente 2",
                Documento = documento,
                Email = "cliente2@email.com"
            };

            // Act
            var response1 = await _client.PostAsJsonAsync("/api/clientes", command1);
            var response2 = await _client.PostAsJsonAsync("/api/clientes", command2);

            // Assert
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result1 = await response1.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();
            var result2 = await response2.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();

            result1!.Success.Should().BeTrue();
            result2!.Success.Should().BeFalse();
            result2.Message.Should().Contain("já cadastrado");
        }

        [Fact]
        public async Task CriarCliente_ComDadosInvalidos_DeveRetornar400()
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = "",
                Documento = DocumentoHelper.GerarCPFValido(),
                Email = "joao@email.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/clientes", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ObterClientePorId_ComIdExistente_DeveRetornar200()
        {
            // Arrange
            var commandCriar = new CriarClienteCommand
            {
                Nome = "Maria Santos",
                Documento = DocumentoHelper.GerarCPFValido(),
                Email = $"maria{Guid.NewGuid()}@email.com"
            };

            var responseCriar = await _client.PostAsJsonAsync("/api/clientes", commandCriar);
            responseCriar.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultCriar = await responseCriar.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();
            resultCriar.Should().NotBeNull();
            resultCriar!.Data.Should().NotBeNull();

            var clienteId = resultCriar.Data!.Id;

            // Act
            var response = await _client.GetAsync($"/api/clientes/{clienteId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();
            result!.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(clienteId);
            result.Data.Nome.Should().Be("Maria Santos");
        }

        [Fact]
        public async Task ObterClientePorId_ComIdInexistente_DeveRetornar404()
        {
            // Arrange
            var clienteId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/clientes/{clienteId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var result = await response.Content.ReadFromJsonAsync<OperationResult<ClienteDTO>>();
            result!.Success.Should().BeFalse();
            result.Message.Should().Contain("não encontrado");
        }
    }
}