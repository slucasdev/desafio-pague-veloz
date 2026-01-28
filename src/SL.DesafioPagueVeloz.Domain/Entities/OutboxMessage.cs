using SL.DesafioPagueVeloz.Domain.Common;

namespace SL.DesafioPagueVeloz.Domain.Entities
{
    public class OutboxMessage : EntityBase
    {
        public string TipoEvento { get; private set; } = string.Empty;
        public string ConteudoJson { get; private set; } = string.Empty;
        public bool Processado { get; private set; }
        public DateTime? ProcessadoEm { get; private set; }
        public int TentativasProcessamento { get; private set; }
        public string? ErroProcessamento { get; private set; }
        public DateTime? ProximaTentativaEm { get; private set; }

        private OutboxMessage() { }

        private OutboxMessage(string tipoEvento, string conteudoJson)
        {
            TipoEvento = tipoEvento;
            ConteudoJson = conteudoJson;
            Processado = false;
            TentativasProcessamento = 0;
        }

        public static OutboxMessage Criar(string tipoEvento, string conteudoJson)
        {
            if (string.IsNullOrWhiteSpace(tipoEvento))
                throw new ArgumentException("Tipo do evento é obrigatório", nameof(tipoEvento));

            if (string.IsNullOrWhiteSpace(conteudoJson))
                throw new ArgumentException("Conteúdo JSON é obrigatório", nameof(conteudoJson));

            return new OutboxMessage(tipoEvento, conteudoJson);
        }

        public void MarcarComoProcessado()
        {
            Processado = true;
            ProcessadoEm = DateTime.UtcNow;
            AtualizarTimestamp();
        }

        public void RegistrarTentativaFalha(string erro, TimeSpan proximaTentativaDelay)
        {
            TentativasProcessamento++;
            ErroProcessamento = erro?.Length > 2000 ? erro.Substring(0, 2000) : erro;
            ProximaTentativaEm = DateTime.UtcNow.Add(proximaTentativaDelay);
            AtualizarTimestamp();
        }

        public bool PodeProcessar()
        {
            return !Processado &&
                   (ProximaTentativaEm == null || ProximaTentativaEm <= DateTime.UtcNow) &&
                   TentativasProcessamento < 5; // Máximo 5 tentativas
        }
    }
}
