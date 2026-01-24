namespace SL.DesafioPagueVeloz.Domain.Events
{
    public abstract record DomainEvent
    {
        public Guid EventId { get; init; }
        public DateTime OcorridoEm { get; init; }
        public string TipoEvento { get; init; }

        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
            OcorridoEm = DateTime.UtcNow;
            TipoEvento = GetType().Name;
        }
    }
}
