using SL.DesafioPagueVeloz.Domain.Enums;

namespace SL.DesafioPagueVeloz.Domain.Events.Transacao
{
    public sealed record TransacaoCriadaEvent : DomainEvent
    {
        public Guid TransacaoId { get; init; }
        public Guid ContaId { get; init; }
        public TipoOperacao TipoOperacao { get; init; }
        public decimal Valor { get; init; }
        public string Descricao { get; init; }
        public Guid IdempotencyKey { get; init; }
        public Guid? TransacaoOrigemId { get; init; }

        public TransacaoCriadaEvent(
            Guid transacaoId,
            Guid contaId,
            TipoOperacao tipoOperacao,
            decimal valor,
            string descricao,
            Guid idempotencyKey,
            Guid? transacaoOrigemId = null)
        {
            TransacaoId = transacaoId;
            ContaId = contaId;
            TipoOperacao = tipoOperacao;
            Valor = valor;
            Descricao = descricao;
            IdempotencyKey = idempotencyKey;
            TransacaoOrigemId = transacaoOrigemId;
        }
    }
}
