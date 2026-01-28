using FluentAssertions;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Enums;

namespace SL.DesafioPagueVeloz.Domain.Tests.Entities
{
    public class TransacaoTests
    {
        [Fact]
        public void Criar_ComDadosValidos_DeveCriarTransacao()
        {
            // Arrange
            var contaId = Guid.NewGuid();
            var tipo = TipoOperacao.Credito;
            var valor = 500m;
            var descricao = "Depósito";
            var idempotencyKey = Guid.NewGuid();

            // Act
            var transacao = Transacao.Criar(contaId, tipo, valor, descricao, idempotencyKey);

            // Assert
            transacao.Should().NotBeNull();
            transacao.Id.Should().NotBeEmpty();
            transacao.ContaId.Should().Be(contaId);
            transacao.Tipo.Should().Be(tipo);
            transacao.Valor.Should().Be(valor);
            transacao.Descricao.Should().Be(descricao);
            transacao.IdempotencyKey.Should().Be(idempotencyKey);
            transacao.Status.Should().Be(StatusTransacao.Pendente);
            transacao.ProcessadoEm.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        [InlineData(-0.01)]
        public void Criar_ComValorInvalido_DeveLancarException(decimal valorInvalido)
        {
            // Arrange
            var contaId = Guid.NewGuid();
            var descricao = "Teste";
            var idempotencyKey = Guid.NewGuid();

            // Act
            Action act = () => Transacao.Criar(contaId, TipoOperacao.Credito, valorInvalido, descricao, idempotencyKey);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Valor deve ser maior que zero*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Criar_ComDescricaoInvalida_DeveLancarException(string? descricaoInvalida)
        {
            // Arrange
            var contaId = Guid.NewGuid();
            var idempotencyKey = Guid.NewGuid();

            // Act
            Action act = () => Transacao.Criar(contaId, TipoOperacao.Credito, 100m, descricaoInvalida!, idempotencyKey);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Descrição é obrigatória*");
        }

        [Fact]
        public void MarcarComoProcessada_DeveAlterarStatus()
        {
            // Arrange
            var transacao = Transacao.Criar(
                Guid.NewGuid(),
                TipoOperacao.Credito,
                500m,
                "Depósito",
                Guid.NewGuid());

            // Act
            transacao.MarcarComoProcessada();

            // Assert
            transacao.Status.Should().Be(StatusTransacao.Processada);
            transacao.ProcessadoEm.Should().NotBeNull();
            transacao.ProcessadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void MarcarComoFalha_DeveAlterarStatusERegistrarMotivo()
        {
            // Arrange
            var transacao = Transacao.Criar(
                Guid.NewGuid(),
                TipoOperacao.Debito,
                100m,
                "Pagamento",
                Guid.NewGuid());
            var motivo = "Saldo insuficiente";

            // Act
            transacao.MarcarComoFalha(motivo);

            // Assert
            transacao.Status.Should().Be(StatusTransacao.Falha);
            transacao.MotivoFalha.Should().Be(motivo);
        }

        [Fact]
        public void MarcarComoEstornada_DeveAlterarStatus()
        {
            // Arrange
            var transacao = Transacao.Criar(
                Guid.NewGuid(),
                TipoOperacao.Debito,
                100m,
                "Pagamento",
                Guid.NewGuid());

            // Act
            transacao.MarcarComoEstornada();

            // Assert
            transacao.Status.Should().Be(StatusTransacao.Estornada);
        }

        [Fact]
        public void Criar_ComTransacaoOrigem_DeveVincular()
        {
            // Arrange
            var contaId = Guid.NewGuid();
            var transacaoOrigemId = Guid.NewGuid();

            // Act
            var transacao = Transacao.Criar(
                contaId,
                TipoOperacao.Estorno,
                100m,
                "Estorno",
                Guid.NewGuid(),
                transacaoOrigemId);

            // Assert
            transacao.TransacaoOrigemId.Should().Be(transacaoOrigemId);
        }
    }
}