using SL.DesafioPagueVeloz.Domain.Enums;

namespace SL.DesafioPagueVeloz.Domain.Events.Transacao
{
    public sealed record TransacaoFalhouEvent : DomainEvent
    {
        public Guid TransacaoId { get; init; }
        public Guid ContaId { get; init; }
        public TipoOperacao TipoOperacao { get; init; }
        public decimal Valor { get; init; }
        public string MotivoFalha { get; init; }
        public string? StackTrace { get; init; }

        public TransacaoFalhouEvent(
            Guid transacaoId,
            Guid contaId,
            TipoOperacao tipoOperacao,
            decimal valor,
            string motivoFalha,
            string? stackTrace = null)
        {
            TransacaoId = transacaoId;
            ContaId = contaId;
            TipoOperacao = tipoOperacao;
            Valor = valor;
            MotivoFalha = motivoFalha;
            StackTrace = stackTrace;
        }
    }
}
