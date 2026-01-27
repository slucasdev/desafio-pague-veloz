using MediatR;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Responses;

namespace SL.DesafioPagueVeloz.Application.Commands
{
    public class EstornarCommand : IRequest<OperationResult<TransacaoDTO>>
    {
        public Guid ContaId { get; set; }
        public decimal Valor { get; set; }
        public Guid TransacaoOriginalId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public Guid IdempotencyKey { get; set; }
    }
}
