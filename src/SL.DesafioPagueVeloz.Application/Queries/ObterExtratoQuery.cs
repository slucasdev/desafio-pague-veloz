using MediatR;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;

namespace SL.DesafioPagueVeloz.Application.Queries
{
    public class ObterExtratoQuery : IRequest<OperationResult<ExtratoDTO>>
    {
        public Guid ContaId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
    }
}
