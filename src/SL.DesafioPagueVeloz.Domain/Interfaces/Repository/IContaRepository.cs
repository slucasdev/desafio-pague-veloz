using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Enums;

namespace SL.DesafioPagueVeloz.Domain.Interfaces.Repository
{
    public interface IContaRepository : IRepositoryBase<Conta>
    {
        Task<Conta?> ObterPorNumeroAsync(string numero, CancellationToken cancellationToken = default);
        Task<Conta?> ObterComTransacoesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Conta?> ObterComLockAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Conta>> ObterPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Conta>> ObterPorStatusAsync(StatusConta status, CancellationToken cancellationToken = default);
        Task<bool> ExisteNumeroAsync(string numero, CancellationToken cancellationToken = default);
    }
}
