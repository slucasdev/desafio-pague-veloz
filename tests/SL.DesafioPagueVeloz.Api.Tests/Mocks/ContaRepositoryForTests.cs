using Microsoft.EntityFrameworkCore;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Context;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories;

namespace SL.DesafioPagueVeloz.Api.Tests.Mocks
{
    /// <summary>
    /// Versão do ContaRepository compatível com SQLite para testes.
    /// Sobrescreve apenas o método ObterComLockAsync que usa SQL Server específico.
    /// </summary>
    public class ContaRepositoryForTests : ContaRepository
    {
        public ContaRepositoryForTests(ApplicationDbContext context) : base(context)
        { }

        public override async Task<Conta?> ObterComLockAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // ✅ SQLite não suporta UPDLOCK/ROWLOCK
            // Para testes, usar query LINQ simples (sem lock)
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
    }
}