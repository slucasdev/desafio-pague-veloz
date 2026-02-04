using SL.DesafioPagueVeloz.Domain.Enums;
using System.Text.RegularExpressions;

namespace SL.DesafioPagueVeloz.Domain.ValueObjects
{
    public sealed record Documento
    {
        public string Numero { get; private init; } = string.Empty;
        public TipoDocumento Tipo { get; private init; }

        private Documento() { }

        private Documento(string numero, TipoDocumento tipo)
        {
            Numero = numero;
            Tipo = tipo;
        }

        // TODO: @slucasdev: Implementar validações melhores para documento, seguindo padrão de mercado
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
            // Exemplo de melhoria comentado abaixo
            return cpf.Length == 11 && cpf.Distinct().Count() > 1;
        }

        //private static bool ValidarCPF(string cpf)
        //{
        //    int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        //    int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        //    string tempCpf = cpf.Substring(0, 9);
        //    int soma = 0;

        //    for (int i = 0; i < 9; i++)
        //        soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        //    int resto = soma % 11;
        //    resto = resto < 2 ? 0 : 11 - resto;

        //    string digito = resto.ToString();
        //    tempCpf += digito;
        //    soma = 0;

        //    for (int i = 0; i < 10; i++)
        //        soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        //    resto = soma % 11;
        //    resto = resto < 2 ? 0 : 11 - resto;

        //    digito += resto.ToString();

        //    return cpf.EndsWith(digito);
        //}

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
