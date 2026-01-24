using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Enums;

namespace SL.DesafioPagueVeloz.Domain.Interfaces.Repository
{
    public interface ITransacaoRepository : IRepositoryBase<Transacao>
    {
        Task<Transacao?> ObterPorIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transacao>> ObterPorContaIdAsync(Guid contaId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transacao>> ObterPorContaIdEPeriodoAsync(
            Guid contaId,
            DateTime dataInicio,
            DateTime dataFim,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<Transacao>> ObterPorStatusAsync(StatusTransacao status, CancellationToken cancellationToken = default);
        Task<bool> ExisteIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default);
    }
}
