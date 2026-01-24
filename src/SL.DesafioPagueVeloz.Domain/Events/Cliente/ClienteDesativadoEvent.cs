namespace SL.DesafioPagueVeloz.Domain.Events.Cliente
{
    public sealed record ClienteDesativadoEvent : DomainEvent
    {
        public Guid ClienteId { get; init; }
        public string Motivo { get; init; }

        public ClienteDesativadoEvent(Guid clienteId, string motivo)
        {
            ClienteId = clienteId;
            Motivo = motivo;
        }
    }
}
