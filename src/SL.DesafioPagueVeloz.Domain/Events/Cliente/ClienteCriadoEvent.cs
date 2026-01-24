namespace SL.DesafioPagueVeloz.Domain.Events.Cliente
{
    public sealed record ClienteCriadoEvent : DomainEvent
    {
        public Guid ClienteId { get; init; }
        public string Nome { get; init; }
        public string Documento { get; init; }
        public string Email { get; init; }

        public ClienteCriadoEvent(Guid clienteId, string nome, string documento, string email)
        {
            ClienteId = clienteId;
            Nome = nome;
            Documento = documento;
            Email = email;
        }
    }
}
