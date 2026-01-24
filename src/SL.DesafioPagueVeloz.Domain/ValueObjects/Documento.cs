using SL.DesafioPagueVeloz.Domain.Enums;
using System.Text.RegularExpressions;

namespace SL.DesafioPagueVeloz.Domain.ValueObjects
{
    public sealed record Documento
    {
        public string Numero { get; }
        public TipoDocumento Tipo { get; }

        private Documento(string numero, TipoDocumento tipo)
        {
            Numero = numero;
            Tipo = tipo;
        }

        public static Documento Criar(string numero)
        {
            var apenasNumeros = Regex.Replace(numero, @"[^\d]", "");

            return apenasNumeros.Length switch
            {
                11 when ValidarCPF(apenasNumeros) => new Documento(apenasNumeros, TipoDocumento.CPF),
                14 when ValidarCNPJ(apenasNumeros) => new Documento(apenasNumeros, TipoDocumento.CNPJ),
                _ => throw new ArgumentException("Documento inválido", nameof(numero))
            };
        }

        private static bool ValidarCPF(string cpf)
        {
            // TODO: @slucasdev - Melhorar validação de CPF
            return cpf.Length == 11 && cpf.Distinct().Count() > 1;
        }

        private static bool ValidarCNPJ(string cnpj)
        {
            // TODO: @slucasdev - Melhorar validação de CNPJ
            return cnpj.Length == 14 && cnpj.Distinct().Count() > 1;
        }

        public string NumeroFormatado => Tipo == TipoDocumento.CPF
            ? Convert.ToUInt64(Numero).ToString(@"000\.000\.000\-00")
            : Convert.ToUInt64(Numero).ToString(@"00\.000\.000\/0000\-00");
    }
}
