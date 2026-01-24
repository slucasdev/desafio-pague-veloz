using SL.DesafioPagueVeloz.Domain.Events;

namespace SL.DesafioPagueVeloz.Domain.Common
{
    public abstract class EntityBase
    {
        public Guid Id { get; protected set; }
        public DateTime CriadoEm { get; protected set; }
        public DateTime? AtualizadoEm { get; protected set; }

        private readonly List<DomainEvent> _domainEvents = new();
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected EntityBase()
        {
            Id = Guid.NewGuid();
            CriadoEm = DateTime.UtcNow;
        }

        protected void AtualizarTimestamp()
        {
            AtualizadoEm = DateTime.UtcNow;
        }

        protected void AdicionarEvento(DomainEvent evento)
        {
            _domainEvents.Add(evento);
        }

        public void LimparEventos()
        {
            _domainEvents.Clear();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not EntityBase other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator == (EntityBase? a, EntityBase? b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator != (EntityBase? a, EntityBase? b) => !(a == b);
    }
}
