using FluentAssertions;
using SL.DesafioPagueVeloz.Domain.Entities;

namespace SL.DesafioPagueVeloz.Domain.Tests.Entities
{
    public class ClienteTests
    {
        [Fact]
        public void Criar_ComDadosValidos_DeveCriarCliente()
        {
            // Arrange
            var nome = "João Silva";
            var documento = "12345678901";
            var email = "joao@email.com";

            // Act
            var cliente = Cliente.Criar(nome, documento, email);

            // Assert
            cliente.Should().NotBeNull();
            cliente.Id.Should().NotBeEmpty();
            cliente.Nome.Should().Be(nome);
            cliente.Documento.Numero.Should().Be(documento);
            cliente.Email.Should().Be(email);
            cliente.Ativo.Should().BeTrue();
            cliente.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Criar_ComNomeInvalido_DeveLancarException(string? nomeInvalido)
        {
            // Arrange
            var documento = "12345678901";
            var email = "joao@email.com";

            // Act
            Action act = () => Cliente.Criar(nomeInvalido!, documento, email);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Nome é obrigatório*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Criar_ComEmailInvalido_DeveLancarException(string? emailInvalido)
        {
            // Arrange
            var nome = "João Silva";
            var documento = "12345678901";

            // Act
            Action act = () => Cliente.Criar(nome, documento, emailInvalido!);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Email é obrigatório*");
        }

        [Fact]
        public void Criar_ComDocumentoInvalido_DeveLancarException()
        {
            // Arrange
            var nome = "João Silva";
            var documento = "123";
            var email = "joao@email.com";

            // Act
            Action act = () => Cliente.Criar(nome, documento, email);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Documento inválido*");
        }

        [Fact]
        public void Desativar_DeveMarcarClienteComoInativo()
        {
            // Arrange
            var cliente = Cliente.Criar("João Silva", "12345678901", "joao@email.com");

            // Act
            cliente.Desativar();

            // Assert
            cliente.Ativo.Should().BeFalse();
        }

        [Fact]
        public void Ativar_DeveMarcarClienteComoAtivo()
        {
            // Arrange
            var cliente = Cliente.Criar("João Silva", "12345678901", "joao@email.com");
            cliente.Desativar();

            // Act
            cliente.Ativar();

            // Assert
            cliente.Ativo.Should().BeTrue();
        }

        [Fact]
        public void AdicionarConta_ComClienteAtivo_DeveAdicionarConta()
        {
            // Arrange
            var cliente = Cliente.Criar("João Silva", "12345678901", "joao@email.com");
            var conta = Conta.Criar(cliente.Id, "00001-5", 1000);

            // Act
            cliente.AdicionarConta(conta);

            // Assert
            cliente.Contas.Should().HaveCount(1);
            cliente.Contas.First().Should().Be(conta);
        }

        [Fact]
        public void AdicionarConta_ComClienteInativo_DeveLancarException()
        {
            // Arrange
            var cliente = Cliente.Criar("João Silva", "12345678901", "joao@email.com");
            cliente.Desativar();
            var conta = Conta.Criar(cliente.Id, "00001-5", 1000);

            // Act
            Action act = () => cliente.AdicionarConta(conta);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cliente inativo não pode ter novas contas");
        }

        [Fact]
        public void Criar_DeveGerarEventoDeDominio()
        {
            // Arrange & Act
            var cliente = Cliente.Criar("João Silva", "12345678901", "joao@email.com");

            // Assert
            cliente.DomainEvents.Should().NotBeEmpty();
            cliente.DomainEvents.Should().Contain(e => e.TipoEvento == "ClienteCriadoEvent");
        }

        [Fact]
        public void Desativar_DeveGerarEventoDeDominio()
        {
            // Arrange
            var cliente = Cliente.Criar("João Silva", "12345678901", "joao@email.com");
            cliente.LimparEventos();

            // Act
            cliente.Desativar();

            // Assert
            cliente.DomainEvents.Should().NotBeEmpty();
            cliente.DomainEvents.Should().Contain(e => e.TipoEvento == "ClienteDesativadoEvent");
        }
    }
}