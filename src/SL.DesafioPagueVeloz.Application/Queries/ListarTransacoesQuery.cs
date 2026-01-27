using MediatR;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;

namespace SL.DesafioPagueVeloz.Application.Queries
{
    public class ListarTransacoesQuery : IRequest<OperationResult<List<TransacaoDTO>>>
    {
        public Guid ContaId { get; set; }
    }
}
