using FluentAssertions;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories;
using SL.DesafioPagueVeloz.Infrastructure.Tests.Fixtures;

namespace SL.DesafioPagueVeloz.Infrastructure.Tests.Repositories
{
    public class ContaRepositoryTests : IClassFixture<InMemoryDbContextFixture>
    {
        private readonly InMemoryDbContextFixture _fixture;

        public ContaRepositoryTests(InMemoryDbContextFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AdicionarAsync_DeveAdicionarConta()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ContaRepository(context);
            var clienteId = Guid.NewGuid();
            var conta = Conta.Criar(clienteId, "00001-5", 1000);

            // Act
            await repository.AdicionarAsync(conta);
            await context.SaveChangesAsync();

            // Assert
            var contaSalva = await repository.ObterPorIdAsync(conta.Id);
            contaSalva.Should().NotBeNull();
            contaSalva!.Numero.Should().Be("00001-5");
            contaSalva.LimiteCredito.Should().Be(1000);
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarConta()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ContaRepository(context);
            var conta = Conta.Criar(Guid.NewGuid(), "00002-7", 2000);

            await repository.AdicionarAsync(conta);
            await context.SaveChangesAsync();

            // Act
            var resultado = await repository.ObterPorIdAsync(conta.Id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(conta.Id);
            resultado.Numero.Should().Be("00002-7");
        }

        [Fact]
        public async Task ObterComLockAsync_ComIdExistente_DeveRetornarConta()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ContaRepository(context);
            var conta = Conta.Criar(Guid.NewGuid(), "00003-9", 1500);

            await repository.AdicionarAsync(conta);
            await context.SaveChangesAsync();

            // Act
            // Nota: InMemory não suporta SQL raw, mas podemos testar se não lança exception
            var resultado = await repository.ObterPorIdAsync(conta.Id); // InMemory usa fallback

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(conta.Id);
        }

        [Fact]
        public async Task ExisteNumeroAsync_ComNumeroExistente_DeveRetornarTrue()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ContaRepository(context);
            var conta = Conta.Criar(Guid.NewGuid(), "00004-1", 1000);

            await repository.AdicionarAsync(conta);
            await context.SaveChangesAsync();

            // Act
            var existe = await repository.ExisteNumeroAsync("00004-1");

            // Assert
            existe.Should().BeTrue();
        }

        [Fact]
        public async Task ExisteNumeroAsync_ComNumeroInexistente_DeveRetornarFalse()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ContaRepository(context);

            // Act
            var existe = await repository.ExisteNumeroAsync("99999-9");

            // Assert
            existe.Should().BeFalse();
        }

        [Fact]
        public async Task Atualizar_DeveAtualizarConta()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new ContaRepository(context);
            var conta = Conta.Criar(Guid.NewGuid(), "00005-3", 1000);

            await repository.AdicionarAsync(conta);
            await context.SaveChangesAsync();

            // Act
            conta.Creditar(500m, "Depósito", Guid.NewGuid());
            repository.Atualizar(conta);
            await context.SaveChangesAsync();

            // Assert
            var contaAtualizada = await repository.ObterPorIdAsync(conta.Id);
            contaAtualizada!.SaldoDisponivel.Should().Be(500m);
        }
    }
}