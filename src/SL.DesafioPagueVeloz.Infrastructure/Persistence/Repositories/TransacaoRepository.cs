using Microsoft.EntityFrameworkCore;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Enums;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Context;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories
{
    public class TransacaoRepository : RepositoryBase<Transacao>, ITransacaoRepository
    {
        public TransacaoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Transacao?> ObterPorIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);
        }

        public async Task<IEnumerable<Transacao>> ObterPorContaIdAsync(Guid contaId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.ContaId == contaId)
                .OrderByDescending(t => t.CriadoEm)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transacao>> ObterPorContaIdEPeriodoAsync(
            Guid contaId,
            DateTime dataInicio,
            DateTime dataFim,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.ContaId == contaId
                         && t.CriadoEm >= dataInicio
                         && t.CriadoEm <= dataFim)
                .OrderByDescending(t => t.CriadoEm)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transacao>> ObterPorStatusAsync(StatusTransacao status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.Status == status)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExisteIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);
        }
    }
}
