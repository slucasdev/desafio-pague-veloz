namespace SL.DesafioPagueVeloz.Application.DTOs
{
    public class TransacaoDTO
    {
        public Guid Id { get; set; }
        public Guid ContaId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid IdempotencyKey { get; set; }
        public Guid? TransacaoOrigemId { get; set; }
        public DateTime? ProcessadoEm { get; set; }
        public string? MotivoFalha { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
