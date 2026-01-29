using FluentAssertions;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Uow;
using SL.DesafioPagueVeloz.Infrastructure.Tests.Fixtures;

namespace SL.DesafioPagueVeloz.Infrastructure.Tests.Persistence
{
    public class UnitOfWorkTests : IClassFixture<InMemoryDbContextFixture>
    {
        private readonly InMemoryDbContextFixture _fixture;

        public UnitOfWorkTests(InMemoryDbContextFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CommitAsync_DeveSalvarTodasAlteracoes()
        {
            // Arrange
            var context = _fixture.CreateNewContext();

            var clienteRepository = new ClienteRepository(context);
            var contaRepository = new ContaRepository(context);
            var transacaoRepository = new TransacaoRepository(context);
            var outboxRepository = new OutboxMessageRepository(context);

            var unitOfWork = new UnitOfWork(
                context,
                clienteRepository,
                contaRepository,
                transacaoRepository,
                outboxRepository);

            var cliente = Cliente.Criar("João Silva", "12345678901", "joao@email.com");
            await unitOfWork.Clientes.AdicionarAsync(cliente);

            // Act
            var result = await unitOfWork.CommitAsync();

            // Assert
            result.Should().BeGreaterThan(0);

            var clienteSalvo = await unitOfWork.Clientes.ObterPorIdAsync(cliente.Id);
            clienteSalvo.Should().NotBeNull();
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_DeveSalvarComSucesso()
        {
            // Arrange
            var context = _fixture.CreateNewContext();

            var clienteRepository = new ClienteRepository(context);
            var contaRepository = new ContaRepository(context);
            var transacaoRepository = new TransacaoRepository(context);
            var outboxRepository = new OutboxMessageRepository(context);

            var unitOfWork = new UnitOfWork(
                context,
                clienteRepository,
                contaRepository,
                transacaoRepository,
                outboxRepository);

            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);

            // Act
            var resultado = await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await unitOfWork.Contas.AdicionarAsync(conta);
                await unitOfWork.CommitAsync();
                return "Sucesso";
            });

            // Assert
            resultado.Should().Be("Sucesso");

            var contaSalva = await unitOfWork.Contas.ObterPorIdAsync(conta.Id);
            contaSalva.Should().NotBeNull();
        }
    }
}