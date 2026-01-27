using MediatR;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;

namespace SL.DesafioPagueVeloz.Application.Queries
{
    public class ObterContaPorIdQuery : IRequest<OperationResult<ContaDTO>>
    {
        public Guid ContaId { get; set; }
    }
}
