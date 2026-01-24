using Microsoft.EntityFrameworkCore;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Context;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories
{
    public class OutboxMessageRepository : RepositoryBase<OutboxMessage>, IOutboxMessageRepository
    {
        public OutboxMessageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<OutboxMessage>> ObterMensagensNaoProcessadasAsync(
            int quantidadeMaxima = 100,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => !m.Processado
                         && m.TentativasProcessamento < 5
                         && (m.ProximaTentativaEm == null || m.ProximaTentativaEm <= DateTime.UtcNow))
                .OrderBy(m => m.CriadoEm)
                .Take(quantidadeMaxima)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OutboxMessage>> ObterMensagensComFalhaAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => !m.Processado && m.TentativasProcessamento >= 5)
                .OrderBy(m => m.CriadoEm)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
