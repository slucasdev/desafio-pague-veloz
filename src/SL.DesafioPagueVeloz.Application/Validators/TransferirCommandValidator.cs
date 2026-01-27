using FluentValidation;
using SL.DesafioPagueVeloz.Application.Commands;

namespace SL.DesafioPagueVeloz.Application.Validators
{
    public class TransferirCommandValidator : AbstractValidator<TransferirCommand>
    {
        public TransferirCommandValidator()
        {
            RuleFor(x => x.ContaOrigemId)
                .NotEmpty().WithMessage("ContaOrigemId é obrigatório");

            RuleFor(x => x.ContaDestinoId)
                .NotEmpty().WithMessage("ContaDestinoId é obrigatório")
                .NotEqual(x => x.ContaOrigemId).WithMessage("Conta origem e destino devem ser diferentes");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("Descrição é obrigatória")
                .MaximumLength(500).WithMessage("Descrição não pode ter mais de 500 caracteres");

            RuleFor(x => x.IdempotencyKey)
                .NotEmpty().WithMessage("IdempotencyKey é obrigatório");
        }
    }
}
