namespace SL.DesafioPagueVeloz.Domain.Events.Conta
{
    public sealed record ContaCriadaEvent : DomainEvent
    {
        public Guid ContaId { get; init; }
        public Guid ClienteId { get; init; }
        public string Numero { get; init; }
        public decimal LimiteCredito { get; init; }

        public ContaCriadaEvent(Guid contaId, Guid clienteId, string numero, decimal limiteCredito)
        {
            ContaId = contaId;
            ClienteId = clienteId;
            Numero = numero;
            LimiteCredito = limiteCredito;
        }
    }
}
