using FluentAssertions;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.Validators;

namespace SL.DesafioPagueVeloz.Application.Tests.Validators
{
    public class CreditarContaCommandValidatorTests
    {
        private readonly CreditarContaCommandValidator _validator;

        public CreditarContaCommandValidatorTests()
        {
            _validator = new CreditarContaCommandValidator();
        }

        [Fact]
        public void Validate_ComDadosValidos_DevePassar()
        {
            // Arrange
            var command = new CreditarContaCommand
            {
                ContaId = Guid.NewGuid(),
                Valor = 500m,
                Descricao = "Depósito inicial",
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ComContaIdVazio_DeveFalhar()
        {
            // Arrange
            var command = new CreditarContaCommand
            {
                ContaId = Guid.Empty,
                Valor = 500m,
                Descricao = "Depósito",
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ContaId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        [InlineData(-0.01)]
        public void Validate_ComValorInvalido_DeveFalhar(decimal valorInvalido)
        {
            // Arrange
            var command = new CreditarContaCommand
            {
                ContaId = Guid.NewGuid(),
                Valor = valorInvalido,
                Descricao = "Depósito",
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Valor");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_ComDescricaoInvalida_DeveFalhar(string? descricaoInvalida)
        {
            // Arrange
            var command = new CreditarContaCommand
            {
                ContaId = Guid.NewGuid(),
                Valor = 500m,
                Descricao = descricaoInvalida!,
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Descricao");
        }

        [Fact]
        public void Validate_ComIdempotencyKeyVazio_DeveFalhar()
        {
            // Arrange
            var command = new CreditarContaCommand
            {
                ContaId = Guid.NewGuid(),
                Valor = 500m,
                Descricao = "Depósito",
                IdempotencyKey = Guid.Empty
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "IdempotencyKey");
        }

        [Fact]
        public void Validate_ComDescricaoMuitoGrande_DeveFalhar()
        {
            // Arrange
            var command = new CreditarContaCommand
            {
                ContaId = Guid.NewGuid(),
                Valor = 500m,
                Descricao = new string('x', 501), // Mais de 500 caracteres
                IdempotencyKey = Guid.NewGuid()
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Descricao");
        }
    }
}