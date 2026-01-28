using FluentAssertions;
using SL.DesafioPagueVeloz.Domain.Enums;
using SL.DesafioPagueVeloz.Domain.ValueObjects;

namespace SL.DesafioPagueVeloz.Domain.Tests.ValueObjects
{
    public class DocumentoTests
    {
        [Fact]
        public void Criar_ComCPFValido_DeveCriarDocumento()
        {
            // Arrange
            var cpf = "12345678901";

            // Act
            var documento = Documento.Criar(cpf);

            // Assert
            documento.Should().NotBeNull();
            documento.Numero.Should().Be(cpf);
            documento.Tipo.Should().Be(TipoDocumento.CPF);
        }

        [Fact]
        public void Criar_ComCPFFormatado_DeveRemoverFormatacao()
        {
            // Arrange
            var cpfFormatado = "123.456.789-01";

            // Act
            var documento = Documento.Criar(cpfFormatado);

            // Assert
            documento.Numero.Should().Be("12345678901");
            documento.Tipo.Should().Be(TipoDocumento.CPF);
        }

        [Fact]
        public void Criar_ComCNPJValido_DeveCriarDocumento()
        {
            // Arrange
            var cnpj = "12345678000190";

            // Act
            var documento = Documento.Criar(cnpj);

            // Assert
            documento.Should().NotBeNull();
            documento.Numero.Should().Be(cnpj);
            documento.Tipo.Should().Be(TipoDocumento.CNPJ);
        }

        [Fact]
        public void Criar_ComCNPJFormatado_DeveRemoverFormatacao()
        {
            // Arrange
            var cnpjFormatado = "12.345.678/0001-90";

            // Act
            var documento = Documento.Criar(cnpjFormatado);

            // Assert
            documento.Numero.Should().Be("12345678000190");
            documento.Tipo.Should().Be(TipoDocumento.CNPJ);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("12345")]
        [InlineData("123456789012345")]
        [InlineData("")]
        public void Criar_ComDocumentoInvalido_DeveLancarException(string? documentoInvalido)
        {
            // Act
            Action act = () => Documento.Criar(documentoInvalido!);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Documento inválido*");
        }

        [Fact]
        public void Criar_ComCPFTodosDigitosIguais_DeveLancarException()
        {
            // Arrange
            var cpfInvalido = "11111111111";

            // Act
            Action act = () => Documento.Criar(cpfInvalido);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void NumeroFormatado_ComCPF_DeveRetornarFormatado()
        {
            // Arrange
            var cpf = "12345678901";
            var documento = Documento.Criar(cpf);

            // Act
            var formatado = documento.NumeroFormatado;

            // Assert
            formatado.Should().Be("123.456.789-01");
        }

        [Fact]
        public void NumeroFormatado_ComCNPJ_DeveRetornarFormatado()
        {
            // Arrange
            var cnpj = "12345678000190";
            var documento = Documento.Criar(cnpj);

            // Act
            var formatado = documento.NumeroFormatado;

            // Assert
            formatado.Should().Be("12.345.678/0001-90");
        }

        [Fact]
        public void DocumentosIguais_DevemSerIguais()
        {
            // Arrange
            var doc1 = Documento.Criar("12345678901");
            var doc2 = Documento.Criar("12345678901");

            // Act & Assert
            doc1.Should().Be(doc2); // Record equality
            (doc1 == doc2).Should().BeTrue();
        }

        [Fact]
        public void DocumentosDiferentes_DevemSerDiferentes()
        {
            // Arrange
            var doc1 = Documento.Criar("12345678901");
            var doc2 = Documento.Criar("98765432100");

            // Act & Assert
            doc1.Should().NotBe(doc2);
            (doc1 != doc2).Should().BeTrue();
        }
    }
}