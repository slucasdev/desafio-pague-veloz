using MediatR;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;

namespace SL.DesafioPagueVeloz.Application.Queries
{
    public class ObterClientePorIdQuery : IRequest<OperationResult<ClienteDTO>>
    {
        public Guid ClienteId { get; set; }
    }
}
