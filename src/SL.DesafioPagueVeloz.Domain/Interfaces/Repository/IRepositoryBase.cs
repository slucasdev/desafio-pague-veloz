using SL.DesafioPagueVeloz.Domain.Common;

namespace SL.DesafioPagueVeloz.Domain.Interfaces.Repository
{
    public interface IRepositoryBase<T> where T : EntityBase
    {
        Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task AdicionarAsync(T entity, CancellationToken cancellationToken = default);
        void Atualizar(T entity);
        void Remover(T entity);
    }
}
