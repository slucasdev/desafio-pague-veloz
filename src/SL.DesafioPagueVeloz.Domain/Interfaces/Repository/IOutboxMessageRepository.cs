using SL.DesafioPagueVeloz.Domain.Entities;

namespace SL.DesafioPagueVeloz.Domain.Interfaces.Repository
{
    public interface IOutboxMessageRepository : IRepositoryBase<OutboxMessage>
    {
        Task<IEnumerable<OutboxMessage>> ObterMensagensNaoProcessadasAsync(int quantidadeMaxima = 100, CancellationToken cancellationToken = default);
        Task<IEnumerable<OutboxMessage>> ObterMensagensComFalhaAsync(CancellationToken cancellationToken = default);
    }
}
