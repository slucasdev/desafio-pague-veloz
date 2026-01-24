using Microsoft.EntityFrameworkCore;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Context;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories
{
    public class ClienteRepository : RepositoryBase<Cliente>, IClienteRepository
    {
        public ClienteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cliente?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Documento.Numero == documento, cancellationToken);
        }

        public async Task<Cliente?> ObterComContasAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Contas)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<bool> ExisteDocumentoAsync(string documento, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(c => c.Documento.Numero == documento, cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObterAtivosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.Ativo)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
