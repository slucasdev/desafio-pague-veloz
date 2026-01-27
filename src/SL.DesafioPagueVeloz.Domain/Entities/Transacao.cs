using SL.DesafioPagueVeloz.Domain.Common;
using SL.DesafioPagueVeloz.Domain.Enums;

namespace SL.DesafioPagueVeloz.Domain.Entities
{
    public class Transacao : EntityBase
    {
        public Guid ContaId { get; private set; }
        public Conta Conta { get; private set; } = null!;
        public TipoOperacao Tipo { get; private set; }
        public decimal Valor { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public StatusTransacao Status { get; private set; }
        public Guid IdempotencyKey { get; private set; }
        public Guid? TransacaoOrigemId { get; private set; }
        public DateTime? ProcessadoEm { get; private set; }
        public string? MotivoFalha { get; private set; }

        private Transacao() { }

        private Transacao(
            Guid contaId,
            TipoOperacao tipo,
            decimal valor,
            string descricao,
            Guid idempotencyKey,
            Guid? transacaoOrigemId = null)
        {
            ContaId = contaId;
            Tipo = tipo;
            Valor = valor;
            Descricao = descricao;
            Status = StatusTransacao.Pendente;
            IdempotencyKey = idempotencyKey;
            TransacaoOrigemId = transacaoOrigemId;
        }

        public static Transacao Criar(
            Guid contaId,
            TipoOperacao tipo,
            decimal valor,
            string descricao,
            Guid idempotencyKey,
            Guid? transacaoOrigemId = null)
        {
            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero", nameof(valor));

            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória", nameof(descricao));

            return new Transacao(contaId, tipo, valor, descricao, idempotencyKey, transacaoOrigemId);
        }

        public void MarcarComoProcessada()
        {
            Status = StatusTransacao.Processada;
            ProcessadoEm = DateTime.UtcNow;
            AtualizarTimestamp();
        }

        public void MarcarComoFalha(string motivo)
        {
            Status = StatusTransacao.Falha;
            MotivoFalha = motivo;
            AtualizarTimestamp();
        }

        public void MarcarComoEstornada()
        {
            Status = StatusTransacao.Estornada;
            AtualizarTimestamp();
        }
    }
}
