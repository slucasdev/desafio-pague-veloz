using FluentAssertions;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories;
using SL.DesafioPagueVeloz.Infrastructure.Tests.Fixtures;

namespace SL.DesafioPagueVeloz.Infrastructure.Tests.Repositories
{
    public class ClienteRepositoryTests : IClassFixture<InMemoryDbContextFixture>
    {
        private readonly InMemoryDbContextFixture _fixture;

        public ClienteRepositoryTests(InMemoryDbContextFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AdicionarAsync_DeveAdicionarCliente()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ClienteRepository(context);
            var cliente = Cliente.Criar("João Silva", "12345678901", "joao@email.com");

            // Act
            await repository.AdicionarAsync(cliente);
            await context.SaveChangesAsync();

            // Assert
            var clienteSalvo = await repository.ObterPorIdAsync(cliente.Id);
            clienteSalvo.Should().NotBeNull();
            clienteSalvo!.Nome.Should().Be("João Silva");
            clienteSalvo.Email.Should().Be("joao@email.com");
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarCliente()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ClienteRepository(context);
            var cliente = Cliente.Criar("Maria Santos", "98765432100", "maria@email.com");

            await repository.AdicionarAsync(cliente);
            await context.SaveChangesAsync();

            // Act
            var resultado = await repository.ObterPorIdAsync(cliente.Id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(cliente.Id);
            resultado.Nome.Should().Be("Maria Santos");
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ClienteRepository(context);

            // Act
            var resultado = await repository.ObterPorIdAsync(Guid.NewGuid());

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ExisteDocumentoAsync_ComDocumentoExistente_DeveRetornarTrue()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ClienteRepository(context);
            var cliente = Cliente.Criar("Pedro Costa", "11122233344", "pedro@email.com");

            await repository.AdicionarAsync(cliente);
            await context.SaveChangesAsync();

            // Act
            var existe = await repository.ExisteDocumentoAsync("11122233344");

            // Assert
            existe.Should().BeTrue();
        }

        [Fact]
        public async Task ExisteDocumentoAsync_ComDocumentoInexistente_DeveRetornarFalse()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ClienteRepository(context);

            // Act
            var existe = await repository.ExisteDocumentoAsync("99988877766");

            // Assert
            existe.Should().BeFalse();
        }

        [Fact]
        public async Task Atualizar_DeveAtualizarCliente()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ClienteRepository(context);
            var cliente = Cliente.Criar("Ana Lima", "55566677788", "ana@email.com");

            await repository.AdicionarAsync(cliente);
            await context.SaveChangesAsync();

            // Act
            cliente.Desativar();
            repository.Atualizar(cliente);
            await context.SaveChangesAsync();

            // Assert
            var clienteAtualizado = await repository.ObterPorIdAsync(cliente.Id);
            clienteAtualizado!.Ativo.Should().BeFalse();
        }
    }
}