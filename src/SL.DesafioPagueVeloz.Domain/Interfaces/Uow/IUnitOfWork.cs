using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;

namespace SL.DesafioPagueVeloz.Domain.Interfaces.Uow
{
    public interface IUnitOfWork : IDisposable
    {
        IClienteRepository Clientes { get; }
        IContaRepository Contas { get; }
        ITransacaoRepository Transacoes { get; }
        IOutboxMessageRepository OutboxMessages { get; }

        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
