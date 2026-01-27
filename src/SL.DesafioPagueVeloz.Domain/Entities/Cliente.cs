using SL.DesafioPagueVeloz.Domain.Common;
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

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório", nameof(email));

            var doc = Documento.Criar(documento);
            return new Cliente(nome, doc, email);
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
            AtualizarTimestamp();
        }

        public void Ativar()
        {
            Ativo = true;
            AtualizarTimestamp();
        }
    }
}
