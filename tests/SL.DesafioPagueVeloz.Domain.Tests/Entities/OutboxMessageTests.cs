using FluentAssertions;
using SL.DesafioPagueVeloz.Domain.Entities;

namespace SL.DesafioPagueVeloz.Domain.Tests.Entities
{
    public class OutboxMessageTests
    {
        [Fact]
        public void Criar_ComDadosValidos_DeveCriarOutboxMessage()
        {
            // Arrange
            var tipoEvento = "TransacaoCriadaEvent";
            var conteudoJson = "{\"TransacaoId\":\"abc\",\"Valor\":500}";

            // Act
            var outboxMessage = OutboxMessage.Criar(tipoEvento, conteudoJson);

            // Assert
            outboxMessage.Should().NotBeNull();
            outboxMessage.Id.Should().NotBeEmpty();
            outboxMessage.TipoEvento.Should().Be(tipoEvento);
            outboxMessage.ConteudoJson.Should().Be(conteudoJson);
            outboxMessage.Processado.Should().BeFalse();
            outboxMessage.ProcessadoEm.Should().BeNull();
            outboxMessage.ErroProcessamento.Should().BeNull();
            outboxMessage.TentativasProcessamento.Should().Be(0);
            outboxMessage.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Criar_ComTipoEventoInvalido_DeveLancarException(string? tipoEventoInvalido)
        {
            // Arrange
            var conteudoJson = "{}";

            // Act
            Action act = () => OutboxMessage.Criar(tipoEventoInvalido!, conteudoJson);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Tipo do evento é obrigatório*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Criar_ComConteudoJsonInvalido_DeveLancarException(string? conteudoInvalido)
        {
            // Arrange
            var tipoEvento = "TransacaoCriadaEvent";

            // Act
            Action act = () => OutboxMessage.Criar(tipoEvento, conteudoInvalido!);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Conteúdo JSON é obrigatório*");
        }

        [Fact]
        public void MarcarComoProcessada_DeveAtualizarPropriedades()
        {
            // Arrange
            var outboxMessage = OutboxMessage.Criar("TransacaoCriadaEvent", "{}");

            // Act
            outboxMessage.MarcarComoProcessado();

            // Assert
            outboxMessage.Processado.Should().BeTrue();
            outboxMessage.ProcessadoEm.Should().NotBeNull();
            outboxMessage.ProcessadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void RegistrarTentativaFalha_DeveIncrementarTentativasEArmazenarErro()
        {
            // Arrange
            var outboxMessage = OutboxMessage.Criar("TransacaoCriadaEvent", "{}");
            var erro = "Falha ao processar evento";
            var delay = TimeSpan.FromSeconds(30);

            // Act
            outboxMessage.RegistrarTentativaFalha(erro, delay);

            // Assert
            outboxMessage.TentativasProcessamento.Should().Be(1);
            outboxMessage.ErroProcessamento.Should().Be(erro);
            outboxMessage.ProximaTentativaEm.Should().NotBeNull();
            outboxMessage.ProximaTentativaEm.Should().BeCloseTo(DateTime.UtcNow.Add(delay), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void RegistrarTentativaFalha_MultiplasVezes_DeveIncrementarContador()
        {
            // Arrange
            var outboxMessage = OutboxMessage.Criar("TransacaoCriadaEvent", "{}");

            // Act
            outboxMessage.RegistrarTentativaFalha("Erro 1", TimeSpan.FromSeconds(2));
            outboxMessage.RegistrarTentativaFalha("Erro 2", TimeSpan.FromSeconds(4));
            outboxMessage.RegistrarTentativaFalha("Erro 3", TimeSpan.FromSeconds(8));

            // Assert
            outboxMessage.TentativasProcessamento.Should().Be(3);
            outboxMessage.ErroProcessamento.Should().Be("Erro 3"); // Último erro
        }

        [Fact]
        public void RegistrarTentativaFalha_ComErroMuitoGrande_DeveTruncarPara2000Caracteres()
        {
            // Arrange
            var outboxMessage = OutboxMessage.Criar("TransacaoCriadaEvent", "{}");
            var erroGrande = new string('x', 3000);

            // Act
            outboxMessage.RegistrarTentativaFalha(erroGrande, TimeSpan.FromSeconds(30));

            // Assert
            outboxMessage.ErroProcessamento.Should().HaveLength(2000);
        }

        [Fact]
        public void PodeProcessar_ComMensagemNova_DeveRetornarTrue()
        {
            // Arrange
            var outboxMessage = OutboxMessage.Criar("TransacaoCriadaEvent", "{}");

            // Act
            var podeProcessar = outboxMessage.PodeProcessar();

            // Assert
            podeProcessar.Should().BeTrue();
        }

        [Fact]
        public void PodeProcessar_ComMensagemProcessada_DeveRetornarFalse()
        {
            // Arrange
            var outboxMessage = OutboxMessage.Criar("TransacaoCriadaEvent", "{}");
            outboxMessage.MarcarComoProcessado();

            // Act
            var podeProcessar = outboxMessage.PodeProcessar();

            // Assert
            podeProcessar.Should().BeFalse();
        }

        [Fact]
        public void PodeProcessar_Com5TentativasFalhadas_DeveRetornarFalse()
        {
            // Arrange
            var outboxMessage = OutboxMessage.Criar("TransacaoCriadaEvent", "{}");

            // Simular 5 tentativas
            for (int i = 0; i < 5; i++)
            {
                outboxMessage.RegistrarTentativaFalha($"Erro {i}", TimeSpan.FromSeconds(Math.Pow(2, i)));
            }

            // Act
            var podeProcessar = outboxMessage.PodeProcessar();

            // Assert
            podeProcessar.Should().BeFalse();
            outboxMessage.TentativasProcessamento.Should().Be(5);
        }

        [Fact]
        public void PodeProcessar_ComProximaTentativaNoFuturo_DeveRetornarFalse()
        {
            // Arrange
            var outboxMessage = OutboxMessage.Criar("TransacaoCriadaEvent", "{}");
            outboxMessage.RegistrarTentativaFalha("Erro temporário", TimeSpan.FromMinutes(5));

            // Act
            var podeProcessar = outboxMessage.PodeProcessar();

            // Assert
            podeProcessar.Should().BeFalse();
        }

        [Fact]
        public void RegistrarTentativaFalha_DeveAtualizarProximaTentativaComDelay()
        {
            // Arrange
            var outboxMessage = OutboxMessage.Criar("TransacaoCriadaEvent", "{}");
            var delay = TimeSpan.FromMinutes(2);
            var expectedTime = DateTime.UtcNow.Add(delay);

            // Act
            outboxMessage.RegistrarTentativaFalha("Timeout", delay);

            // Assert
            outboxMessage.ProximaTentativaEm.Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(2));
        }
    }
}