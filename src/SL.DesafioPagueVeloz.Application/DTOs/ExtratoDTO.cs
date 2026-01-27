namespace SL.DesafioPagueVeloz.Application.DTOs
{
    public class ExtratoDTO
    {
        public Guid ContaId { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoFinal { get; set; }
        public List<TransacaoDTO> Transacoes { get; set; } = new();
        public int TotalTransacoes { get; set; }
    }
}
