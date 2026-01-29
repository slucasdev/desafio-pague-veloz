using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Infrastructure.Tests.Fixtures;

namespace SL.DesafioPagueVeloz.Infrastructure.Tests.Persistence
{
    public class OutboxPatternTests : IClassFixture<InMemoryDbContextFixture>
    {
        private readonly InMemoryDbContextFixture _fixture;

        public OutboxPatternTests(InMemoryDbContextFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SaveChangesAsync_ComEntidadeComEventos_DeveSalvarNaOutbox()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Creditar(500m, "Depósito", Guid.NewGuid());

            // Conta tem 2 eventos: TransacaoCriadaEvent + SaldoAtualizadoEvent
            conta.DomainEvents.Should().HaveCountGreaterThanOrEqualTo(2);

            // Act
            context.Contas.Add(conta);
            await context.SaveChangesAsync();

            // Assert - Verificar se OutboxMessages foram criadas
            var outboxMessages = await context.OutboxMessages.ToListAsync();
            outboxMessages.Should().HaveCountGreaterThanOrEqualTo(2);

            // Verificar tipos de eventos
            outboxMessages.Should().Contain(m => m.TipoEvento == "TransacaoCriadaEvent");
            outboxMessages.Should().Contain(m => m.TipoEvento == "SaldoAtualizadoEvent");

            // Todas devem estar não processadas
            outboxMessages.Should().AllSatisfy(m => m.Processado.Should().BeFalse());
        }

        [Fact]
        public async Task SaveChangesAsync_DeveLimparEventosDasEntidades()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var conta = Conta.Criar(Guid.NewGuid(), "00002-7", 1000);
            conta.Creditar(500m, "Depósito", Guid.NewGuid());

            conta.DomainEvents.Should().NotBeEmpty();

            // Act
            context.Contas.Add(conta);
            await context.SaveChangesAsync();

            // Assert - Eventos devem ser limpos após SaveChanges
            conta.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public async Task OutboxMessage_DeveSerializarEventoCorretamente()
        {
            // Arrange
            var context = _fixture.CreateNewContext();

            var nome = "João Silva";
            var documento = "12345678901";
            var email = "joao@email.com";

            var cliente = Cliente.Criar(nome, documento, email);

            // Act
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            // Assert
            var outboxMessages = await context.OutboxMessages.ToListAsync();
            outboxMessages.Should().HaveCount(1);

            var message = outboxMessages.First();
            message.TipoEvento.Should().Be("ClienteCriadoEvent");
            message.ConteudoJson.Should().Contain("ClienteId");
            message.ConteudoJson.Should().Contain(documento);
            message.ConteudoJson.Should().Contain(email);
        }

        [Fact]
        public async Task MultiplaOperacoes_DeveSalvarTodosEventosNaOutbox()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var clienteId = Guid.NewGuid();
            var cliente = Cliente.Criar("Maria Santos", "98765432100", "maria@email.com");
            var conta = Conta.Criar(clienteId, "00003-9", 1000);

            conta.Creditar(1000m, "Depósito", Guid.NewGuid());
            conta.Debitar(200m, "Pagamento", Guid.NewGuid());

            // Act
            context.Clientes.Add(cliente);
            context.Contas.Add(conta);
            await context.SaveChangesAsync();

            // Assert
            var outboxMessages = await context.OutboxMessages.ToListAsync();

            // Cliente (1 evento) + Conta (1 evento de criação) + 2 operações (4 eventos)
            // = no mínimo 6 eventos
            outboxMessages.Should().HaveCountGreaterThanOrEqualTo(6);

            outboxMessages.Should().Contain(m => m.TipoEvento == "ClienteCriadoEvent");
            outboxMessages.Should().Contain(m => m.TipoEvento == "ContaCriadaEvent");
            outboxMessages.Should().Contain(m => m.TipoEvento == "TransacaoCriadaEvent");
            outboxMessages.Should().Contain(m => m.TipoEvento == "SaldoAtualizadoEvent");
        }
    }
}