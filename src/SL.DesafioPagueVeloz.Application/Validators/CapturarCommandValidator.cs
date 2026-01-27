using FluentValidation;
using SL.DesafioPagueVeloz.Application.Commands;

namespace SL.DesafioPagueVeloz.Application.Validators
{
    public class CapturarCommandValidator : AbstractValidator<CapturarCommand>
    {
        public CapturarCommandValidator()
        {
            RuleFor(x => x.ContaId)
                .NotEmpty().WithMessage("ContaId é obrigatório");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

            RuleFor(x => x.TransacaoReservaId)
                .NotEmpty().WithMessage("TransacaoReservaId é obrigatório");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("Descrição é obrigatória")
                .MaximumLength(500).WithMessage("Descrição não pode ter mais de 500 caracteres");

            RuleFor(x => x.IdempotencyKey)
                .NotEmpty().WithMessage("IdempotencyKey é obrigatório");
        }
    }
}
