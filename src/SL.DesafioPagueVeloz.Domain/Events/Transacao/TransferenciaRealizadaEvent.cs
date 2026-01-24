namespace SL.DesafioPagueVeloz.Domain.Events.Transacao
{
    public sealed record TransferenciaRealizadaEvent : DomainEvent
    {
        public Guid TransacaoId { get; init; }
        public Guid ContaOrigemId { get; init; }
        public Guid ContaDestinoId { get; init; }
        public decimal Valor { get; init; }
        public string Descricao { get; init; }

        public TransferenciaRealizadaEvent(
            Guid transacaoId,
            Guid contaOrigemId,
            Guid contaDestinoId,
            decimal valor,
            string descricao)
        {
            TransacaoId = transacaoId;
            ContaOrigemId = contaOrigemId;
            ContaDestinoId = contaDestinoId;
            Valor = valor;
            Descricao = descricao;
        }
    }
}
