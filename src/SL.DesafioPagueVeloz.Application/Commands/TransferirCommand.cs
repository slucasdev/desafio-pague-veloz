using MediatR;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;

namespace SL.DesafioPagueVeloz.Application.Commands
{
    public class TransferirCommand : IRequest<OperationResult<List<TransacaoDTO>>>
    {
        public Guid ContaOrigemId { get; set; }
        public Guid ContaDestinoId { get; set; }
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public Guid IdempotencyKey { get; set; }
    }
}
