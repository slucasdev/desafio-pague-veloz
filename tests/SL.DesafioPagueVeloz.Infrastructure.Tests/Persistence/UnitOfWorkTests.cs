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
        public async Task BeginTransactionAsync_DeveGerenciarTransacaoComSucesso()
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
            await unitOfWork.BeginTransactionAsync();
            await unitOfWork.Contas.AdicionarAsync(conta);
            await unitOfWork.CommitAsync();

            // Assert
            var contaSalva = await unitOfWork.Contas.ObterPorIdAsync(conta.Id);
            contaSalva.Should().NotBeNull();
            contaSalva!.Numero.Should().Be("00001-5");
            contaSalva.LimiteCredito.Should().Be(1000);
        }

        [Fact]
        public async Task RollbackAsync_DeveReverterAlteracoes()
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

            var conta = Conta.Criar(Guid.NewGuid(), "00002-3", 500);

            // Act
            await unitOfWork.BeginTransactionAsync();
            await unitOfWork.Contas.AdicionarAsync(conta);
            await unitOfWork.RollbackAsync();

            // Assert
            var contaSalva = await unitOfWork.Contas.ObterPorIdAsync(conta.Id);
            contaSalva.Should().BeNull();
        }

        [Fact]
        public async Task CommitAsync_SemTransacao_DeveSalvarNormalmente()
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

            var cliente = Cliente.Criar("Maria Santos", "98765432100", "maria@email.com");
            await unitOfWork.Clientes.AdicionarAsync(cliente);

            // Act (sem BeginTransaction)
            var result = await unitOfWork.CommitAsync();

            // Assert
            result.Should().BeGreaterThan(0);
            var clienteSalvo = await unitOfWork.Clientes.ObterPorIdAsync(cliente.Id);
            clienteSalvo.Should().NotBeNull();
        }
    }
}