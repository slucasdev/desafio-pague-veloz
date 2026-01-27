using MediatR;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;

namespace SL.DesafioPagueVeloz.Application.Commands
{
    public class CriarContaCommand : IRequest<OperationResult<ContaDTO>>
    {
        public Guid ClienteId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
    }
}
