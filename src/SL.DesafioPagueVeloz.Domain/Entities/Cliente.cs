using SL.DesafioPagueVeloz.Domain.Common;
using SL.DesafioPagueVeloz.Domain.Events.Cliente;
using SL.DesafioPagueVeloz.Domain.ValueObjects;

namespace SL.DesafioPagueVeloz.Domain.Entities
{
    public class Cliente : EntityBase
    {
        public string Nome { get; private set; } = string.Empty;
        public Documento Documento { get; private set; } = null!;
        public string Email { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }

        private readonly List<Conta> _contas = new();
        public IReadOnlyCollection<Conta> Contas => _contas.AsReadOnly();

        private Cliente() { }

        private Cliente(string nome, Documento documento, string email)
        {
            Nome = nome;
            Documento = documento;
            Email = email;
            Ativo = true;
        }

        public static Cliente Criar(string nome, string documento, string email)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório", nameof(nome));

            // TODO: @slucasdev - Implementar validações mais robustas para o email, seguinte padrão de mercado
            // Ex: esta comentado o metodo: IsEmailValido
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório", nameof(email));

            var doc = Documento.Criar(documento);
            var cliente = new Cliente(nome, doc, email);

            cliente.AdicionarEvento(new ClienteCriadoEvent(cliente.Id, nome, email, doc.Numero));

            return cliente;
        }

        public void AdicionarConta(Conta conta)
        {
            if (!Ativo)
                throw new InvalidOperationException("Cliente inativo não pode ter novas contas");

            _contas.Add(conta);
            AtualizarTimestamp();
        }

        public void Desativar()
        {
            Ativo = false;

            AdicionarEvento(new ClienteDesativadoEvent(Id, "Cliente desativado manualmente"));

            AtualizarTimestamp();
        }

        public void Ativar()
        {
            Ativo = true;
            AtualizarTimestamp();
        }

        //private static bool IsEmailValido(string email)
        //{
        //    try
        //    {
        //        var addr = new System.Net.Mail.MailAddress(email);
        //        return addr.Address == email;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
    }
}
