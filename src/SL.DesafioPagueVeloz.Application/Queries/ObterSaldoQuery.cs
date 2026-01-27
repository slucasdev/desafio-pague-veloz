using MediatR;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;

namespace SL.DesafioPagueVeloz.Application.Queries
{
    public class ObterSaldoQuery : IRequest<OperationResult<SaldoDTO>>
    {
        public Guid ContaId { get; set; }
    }
}
