using SL.DesafioPagueVeloz.Domain.Events;

namespace SL.DesafioPagueVeloz.Application.Interfaces
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
        Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
