namespace SL.DesafioPagueVeloz.Application.DTOs
{
    public class SaldoDTO
    {
        public Guid ContaId { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public decimal SaldoDisponivel { get; set; }
        public decimal SaldoReservado { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal SaldoTotal { get; set; }
        public DateTime ConsultadoEm { get; set; }
    }
}
