using SL.DesafioPagueVeloz.Domain.Enums;

namespace SL.DesafioPagueVeloz.Domain.Events.Conta
{
    public sealed record ContaBloqueadaEvent : DomainEvent
    {
        public Guid ContaId { get; init; }
        public string Numero { get; init; }
        public string Motivo { get; init; }
        public StatusConta StatusAnterior { get; init; }

        public ContaBloqueadaEvent(Guid contaId, string numero, string motivo, StatusConta statusAnterior)
        {
            ContaId = contaId;
            Numero = numero;
            Motivo = motivo;
            StatusAnterior = statusAnterior;
        }
    }
}
