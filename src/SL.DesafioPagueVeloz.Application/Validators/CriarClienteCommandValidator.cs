using FluentValidation;
using SL.DesafioPagueVeloz.Application.Commands;

namespace SL.DesafioPagueVeloz.Application.Validators
{
    public class CriarClienteCommandValidator : AbstractValidator<CriarClienteCommand>
    {
        public CriarClienteCommandValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MaximumLength(200).WithMessage("Nome não pode ter mais de 200 caracteres");

            RuleFor(x => x.Documento)
                .NotEmpty().WithMessage("Documento é obrigatório")
                .Must(BeValidDocumento).WithMessage("Documento inválido");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email é obrigatório")
                .EmailAddress().WithMessage("Email inválido")
                .MaximumLength(255).WithMessage("Email não pode ter mais de 255 caracteres");
        }

        private bool BeValidDocumento(string documento)
        {
            var apenasNumeros = new string(documento.Where(char.IsDigit).ToArray());
            return apenasNumeros.Length == 11 || apenasNumeros.Length == 14;
        }
    }
}
