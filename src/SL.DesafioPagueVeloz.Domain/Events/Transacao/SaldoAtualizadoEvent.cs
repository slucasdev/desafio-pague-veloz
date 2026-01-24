namespace SL.DesafioPagueVeloz.Domain.Events.Transacao
{
    public sealed record SaldoAtualizadoEvent : DomainEvent
    {
        public Guid ContaId { get; init; }
        public decimal SaldoDisponivelAnterior { get; init; }
        public decimal SaldoDisponivelAtual { get; init; }
        public decimal SaldoReservadoAnterior { get; init; }
        public decimal SaldoReservadoAtual { get; init; }
        public decimal Diferenca { get; init; }

        public SaldoAtualizadoEvent(
            Guid contaId,
            decimal saldoDisponivelAnterior,
            decimal saldoDisponivelAtual,
            decimal saldoReservadoAnterior,
            decimal saldoReservadoAtual)
        {
            ContaId = contaId;
            SaldoDisponivelAnterior = saldoDisponivelAnterior;
            SaldoDisponivelAtual = saldoDisponivelAtual;
            SaldoReservadoAnterior = saldoReservadoAnterior;
            SaldoReservadoAtual = saldoReservadoAtual;
            Diferenca = (saldoDisponivelAtual + saldoReservadoAtual) -
                        (saldoDisponivelAnterior + saldoReservadoAnterior);
        }
    }
}
