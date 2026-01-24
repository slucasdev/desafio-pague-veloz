using SL.DesafioPagueVeloz.Domain.Enums;

namespace SL.DesafioPagueVeloz.Domain.Events.Transacao
{
    public sealed record TransacaoProcessadaEvent : DomainEvent
    {
        public Guid TransacaoId { get; init; }
        public Guid ContaId { get; init; }
        public TipoOperacao TipoOperacao { get; init; }
        public decimal Valor { get; init; }
        public decimal SaldoDisponivel { get; init; }
        public decimal SaldoReservado { get; init; }

        public TransacaoProcessadaEvent(
            Guid transacaoId,
            Guid contaId,
            TipoOperacao tipoOperacao,
            decimal valor,
            decimal saldoDisponivel,
            decimal saldoReservado)
        {
            TransacaoId = transacaoId;
            ContaId = contaId;
            TipoOperacao = tipoOperacao;
            Valor = valor;
            SaldoDisponivel = saldoDisponivel;
            SaldoReservado = saldoReservado;
        }
    }
}
