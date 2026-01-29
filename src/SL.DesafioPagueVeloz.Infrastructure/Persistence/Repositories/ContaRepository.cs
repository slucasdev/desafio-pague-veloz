using Microsoft.EntityFrameworkCore;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Enums;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Context;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories
{
    public class ContaRepository : RepositoryBase<Conta>, IContaRepository
    {
        public ContaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Conta?> ObterPorNumeroAsync(string numero, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Numero == numero, cancellationToken);
        }

        public async Task<Conta?> ObterComTransacoesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Transacoes.OrderByDescending(t => t.CriadoEm).Take(100))
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public virtual async Task<Conta?> ObterComLockAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // UPDLOCK + ROWLOCK: Lock pessimista para garantir exclusividade na operação
            // Isso previne condições de corrida em operações concorrentes na mesma conta
            return await _dbSet
                .FromSqlRaw(@"
                SELECT * FROM Contas WITH (UPDLOCK, ROWLOCK) 
                WHERE Id = {0}", id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<Conta>> ObterPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.ClienteId == clienteId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Conta>> ObterPorStatusAsync(StatusConta status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.Status == status)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExisteNumeroAsync(string numero, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(c => c.Numero == numero, cancellationToken);
        }
    }
}
