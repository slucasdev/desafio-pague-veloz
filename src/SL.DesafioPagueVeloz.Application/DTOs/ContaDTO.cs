namespace SL.DesafioPagueVeloz.Application.DTOs
{
    public class ContaDTO
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public decimal SaldoDisponivel { get; set; }
        public decimal SaldoReservado { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal SaldoTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public DateTime? AtualizadoEm { get; set; }
    }
}
