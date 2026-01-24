using SL.DesafioPagueVeloz.Domain.Entities;

namespace SL.DesafioPagueVeloz.Domain.Interfaces.Repository
{
    public interface IClienteRepository : IRepositoryBase<Cliente>
    {
        Task<Cliente?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default);
        Task<Cliente?> ObterComContasAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExisteDocumentoAsync(string documento, CancellationToken cancellationToken = default);
        Task<IEnumerable<Cliente>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    }
}
