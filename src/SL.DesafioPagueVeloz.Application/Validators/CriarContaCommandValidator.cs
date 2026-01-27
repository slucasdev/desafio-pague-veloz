using FluentValidation;
using SL.DesafioPagueVeloz.Application.Commands;

namespace SL.DesafioPagueVeloz.Application.Validators
{
    public class CriarContaCommandValidator : AbstractValidator<CriarContaCommand>
    {
        public CriarContaCommandValidator()
        {
            RuleFor(x => x.ClienteId)
                .NotEmpty().WithMessage("ClienteId é obrigatório");

            RuleFor(x => x.Numero)
                .NotEmpty().WithMessage("Número da conta é obrigatório")
                .MaximumLength(20).WithMessage("Número da conta não pode ter mais de 20 caracteres");

            RuleFor(x => x.LimiteCredito)
                .GreaterThanOrEqualTo(0).WithMessage("Limite de crédito não pode ser negativo");
        }
    }
}
