using FluentAssertions;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.Validators;

namespace SL.DesafioPagueVeloz.Application.Tests.Validators
{
    public class CriarClienteCommandValidatorTests
    {
        private readonly CriarClienteCommandValidator _validator;

        public CriarClienteCommandValidatorTests()
        {
            _validator = new CriarClienteCommandValidator();
        }

        [Fact]
        public void Validate_ComDadosValidos_DevePassar()
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = "João Silva",
                Documento = "12345678901",
                Email = "joao@email.com"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_ComNomeInvalido_DeveFalhar(string? nomeInvalido)
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = nomeInvalido!,
                Documento = "12345678901",
                Email = "joao@email.com"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Nome");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_ComDocumentoInvalido_DeveFalhar(string? documentoInvalido)
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = "João Silva",
                Documento = documentoInvalido!,
                Email = "joao@email.com"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Documento");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_ComEmailInvalido_DeveFalhar(string? emailInvalido)
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = "João Silva",
                Documento = "12345678901",
                Email = emailInvalido!
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_ComEmailFormatoInvalido_DeveFalhar()
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = "João Silva",
                Documento = "12345678901",
                Email = "email-invalido"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_ComNomeMuitoGrande_DeveFalhar()
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = new string('x', 201), // Mais de 200 caracteres
                Documento = "12345678901",
                Email = "joao@email.com"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Nome");
        }
    }
}