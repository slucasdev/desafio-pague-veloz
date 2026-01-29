using FluentAssertions;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Enums;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories;
using SL.DesafioPagueVeloz.Infrastructure.Tests.Fixtures;

namespace SL.DesafioPagueVeloz.Infrastructure.Tests.Repositories
{
    public class TransacaoRepositoryTests : IClassFixture<InMemoryDbContextFixture>
    {
        private readonly InMemoryDbContextFixture _fixture;

        public TransacaoRepositoryTests(InMemoryDbContextFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AdicionarAsync_DeveAdicionarTransacao()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new TransacaoRepository(context);
            var contaId = Guid.NewGuid();
            var transacao = Transacao.Criar(contaId, TipoOperacao.Credito, 500m, "Depósito", Guid.NewGuid());

            // Act
            await repository.AdicionarAsync(transacao);
            await context.SaveChangesAsync();

            // Assert
            var transacaoSalva = await repository.ObterPorIdAsync(transacao.Id);
            transacaoSalva.Should().NotBeNull();
            transacaoSalva!.Valor.Should().Be(500m);
        }

        [Fact]
        public async Task ObterPorIdempotencyKeyAsync_ComKeyExistente_DeveRetornarTransacao()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new TransacaoRepository(context);
            var idempotencyKey = Guid.NewGuid();
            var transacao = Transacao.Criar(Guid.NewGuid(), TipoOperacao.Credito, 500m, "Depósito", idempotencyKey);

            await repository.AdicionarAsync(transacao);
            await context.SaveChangesAsync();

            // Act
            var resultado = await repository.ObterPorIdempotencyKeyAsync(idempotencyKey);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.IdempotencyKey.Should().Be(idempotencyKey);
        }

        [Fact]
        public async Task ObterPorIdempotencyKeyAsync_ComKeyInexistente_DeveRetornarNull()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new TransacaoRepository(context);

            // Act
            var resultado = await repository.ObterPorIdempotencyKeyAsync(Guid.NewGuid());

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObterPorContaIdAsync_DeveRetornarTodasTransacoesDaConta()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new TransacaoRepository(context);
            var contaId = Guid.NewGuid();

            var transacao1 = Transacao.Criar(contaId, TipoOperacao.Credito, 500m, "Depósito 1", Guid.NewGuid());
            var transacao2 = Transacao.Criar(contaId, TipoOperacao.Debito, 100m, "Pagamento 1", Guid.NewGuid());
            var transacao3 = Transacao.Criar(Guid.NewGuid(), TipoOperacao.Credito, 300m, "Outra conta", Guid.NewGuid());

            await repository.AdicionarAsync(transacao1);
            await repository.AdicionarAsync(transacao2);
            await repository.AdicionarAsync(transacao3);
            await context.SaveChangesAsync();

            // Act
            var resultado = await repository.ObterPorContaIdAsync(contaId);

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().Contain(t => t.Id == transacao1.Id);
            resultado.Should().Contain(t => t.Id == transacao2.Id);
            resultado.Should().NotContain(t => t.Id == transacao3.Id);
        }

        [Fact]
        public async Task ObterPorContaIdEPeriodoAsync_DeveRetornarTransacoesNoPeriodo()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new TransacaoRepository(context);
            var contaId = Guid.NewGuid();

            var transacao1 = Transacao.Criar(contaId, TipoOperacao.Credito, 500m, "Depósito", Guid.NewGuid());
            await repository.AdicionarAsync(transacao1);
            await context.SaveChangesAsync();

            var dataInicio = DateTime.UtcNow.AddDays(-7);
            var dataFim = DateTime.UtcNow.AddDays(1);

            // Act
            var resultado = await repository.ObterPorContaIdEPeriodoAsync(contaId, dataInicio, dataFim);

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().Id.Should().Be(transacao1.Id);
        }

        [Fact]
        public async Task Atualizar_DeveAtualizarTransacao()
        {
            // Arrange
            var context = _fixture.CreateNewContext();
            var repository = new TransacaoRepository(context);
            var transacao = Transacao.Criar(Guid.NewGuid(), TipoOperacao.Credito, 500m, "Depósito", Guid.NewGuid());

            await repository.AdicionarAsync(transacao);
            await context.SaveChangesAsync();

            // Act
            transacao.MarcarComoProcessada();
            repository.Atualizar(transacao);
            await context.SaveChangesAsync();

            // Assert
            var transacaoAtualizada = await repository.ObterPorIdAsync(transacao.Id);
            transacaoAtualizada!.Status.Should().Be(StatusTransacao.Processada);
            transacaoAtualizada.ProcessadoEm.Should().NotBeNull();
        }
    }
}