using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Context;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Uow
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(
            ApplicationDbContext context,
            IClienteRepository clientes,
            IContaRepository contas,
            ITransacaoRepository transacoes,
            IOutboxMessageRepository outboxMessages)
        {
            _context = context;
            Clientes = clientes;
            Contas = contas;
            Transacoes = transacoes;
            OutboxMessages = outboxMessages;
        }

        public IClienteRepository Clientes { get; }
        public IContaRepository Contas { get; }
        public ITransacaoRepository Transacoes { get; }
        public IOutboxMessageRepository OutboxMessages { get; }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            var result = await _context.SaveChangesAsync(cancellationToken);

            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            return result;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task<T> ExecuteInTransactionWithRetryAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await BeginTransactionAsync(cancellationToken);

                try
                {
                    var result = await action();
                    await CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
