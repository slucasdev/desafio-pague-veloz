using SL.DesafioPagueVeloz.Domain.Common;
using SL.DesafioPagueVeloz.Domain.Enums;
using SL.DesafioPagueVeloz.Domain.Events.Conta;
using SL.DesafioPagueVeloz.Domain.Events.Transacao;
using SL.DesafioPagueVeloz.Domain.Exceptions;

namespace SL.DesafioPagueVeloz.Domain.Entities
{
    public class Conta : EntityBase
    {
        public Guid ClienteId { get; private set; }
        public Cliente Cliente { get; private set; } = null!;
        public string Numero { get; private set; } = string.Empty;
        public decimal SaldoDisponivel { get; private set; }
        public decimal SaldoReservado { get; private set; }
        public decimal LimiteCredito { get; private set; }
        public StatusConta Status { get; private set; }

        private readonly List<Transacao> _transacoes = new();
        public IReadOnlyCollection<Transacao> Transacoes => _transacoes.AsReadOnly();

        private Conta() { }

        private Conta(Guid clienteId, string numero, decimal limiteCredito)
        {
            ClienteId = clienteId;
            Numero = numero;
            SaldoDisponivel = 0;
            SaldoReservado = 0;
            LimiteCredito = limiteCredito;
            Status = StatusConta.Ativa;
        }

        public static Conta Criar(Guid clienteId, string numero, decimal limiteCredito = 0)
        {
            if (string.IsNullOrWhiteSpace(numero))
                throw new ArgumentException("Número da conta é obrigatório", nameof(numero));

            if (limiteCredito < 0)
                throw new ArgumentException("Limite de crédito não pode ser negativo", nameof(limiteCredito));

            var conta = new Conta(clienteId, numero, limiteCredito);
            conta.AdicionarEvento(new ContaCriadaEvent(conta.Id, clienteId, numero, limiteCredito));
            return conta;
        }

        public decimal SaldoTotal => SaldoDisponivel + LimiteCredito;

        public void Creditar(decimal valor, string descricao, Guid idempotencyKey)
        {
            ValidarOperacao(valor);

            var saldoAnterior = SaldoDisponivel;
            SaldoDisponivel += valor;

            var transacao = Transacao.Criar(Id, TipoOperacao.Credito, valor, descricao, idempotencyKey);
            _transacoes.Add(transacao);

            AdicionarEvento(new TransacaoCriadaEvent(transacao.Id, Id, TipoOperacao.Credito, valor, descricao, idempotencyKey));
            AdicionarEvento(new SaldoAtualizadoEvent(Id, saldoAnterior, SaldoDisponivel, SaldoReservado, SaldoReservado));

            AtualizarTimestamp();
        }

        public void Debitar(decimal valor, string descricao, Guid idempotencyKey)
        {
            ValidarOperacao(valor);

            if (SaldoTotal < valor)
                throw new SaldoInsuficienteException($"Saldo insuficiente. Disponível: {SaldoTotal:C2}");

            var saldoAnterior = SaldoDisponivel;
            SaldoDisponivel -= valor;

            var transacao = Transacao.Criar(Id, TipoOperacao.Debito, valor, descricao, idempotencyKey);
            _transacoes.Add(transacao);

            AdicionarEvento(new TransacaoCriadaEvent(transacao.Id, Id, TipoOperacao.Debito, valor, descricao, idempotencyKey));
            AdicionarEvento(new SaldoAtualizadoEvent(Id, saldoAnterior, SaldoDisponivel, SaldoReservado, SaldoReservado));

            AtualizarTimestamp();
        }

        public void Reservar(decimal valor, string descricao, Guid idempotencyKey)
        {
            ValidarOperacao(valor);

            if (SaldoTotal < valor)
                throw new SaldoInsuficienteException($"Saldo insuficiente para reserva. Disponível: {SaldoTotal:C2}");

            var saldoDisponivelAnterior = SaldoDisponivel;
            var saldoReservadoAnterior = SaldoReservado;

            SaldoDisponivel -= valor;
            SaldoReservado += valor;

            var transacao = Transacao.Criar(Id, TipoOperacao.Reserva, valor, descricao, idempotencyKey);
            _transacoes.Add(transacao);

            AdicionarEvento(new TransacaoCriadaEvent(transacao.Id, Id, TipoOperacao.Reserva, valor, descricao, idempotencyKey));
            AdicionarEvento(new SaldoAtualizadoEvent(Id, saldoDisponivelAnterior, SaldoDisponivel, saldoReservadoAnterior, SaldoReservado));

            AtualizarTimestamp();
        }

        public void Capturar(decimal valor, Guid transacaoReservaId, string descricao, Guid idempotencyKey)
        {
            ValidarOperacao(valor);

            if (SaldoReservado < valor)
                throw new InvalidOperationException($"Saldo reservado insuficiente. Reservado: {SaldoReservado:C2}");

            var saldoDisponivelAnterior = SaldoDisponivel;
            var saldoReservadoAnterior = SaldoReservado;

            SaldoReservado -= valor;

            var transacao = Transacao.Criar(Id, TipoOperacao.Captura, valor, descricao, idempotencyKey, transacaoReservaId);
            _transacoes.Add(transacao);

            AdicionarEvento(new TransacaoCriadaEvent(transacao.Id, Id, TipoOperacao.Captura, valor, descricao, idempotencyKey, transacaoReservaId));
            AdicionarEvento(new SaldoAtualizadoEvent(Id, saldoDisponivelAnterior, SaldoDisponivel, saldoReservadoAnterior, SaldoReservado));

            AtualizarTimestamp();
        }

        public void CancelarReserva(decimal valor, Guid transacaoReservaId, string descricao, Guid idempotencyKey)
        {
            ValidarOperacao(valor);

            if (SaldoReservado < valor)
                throw new InvalidOperationException($"Saldo reservado insuficiente. Reservado: {SaldoReservado:C2}");

            var saldoDisponivelAnterior = SaldoDisponivel;
            var saldoReservadoAnterior = SaldoReservado;

            SaldoReservado -= valor;
            SaldoDisponivel += valor;

            var transacao = Transacao.Criar(Id, TipoOperacao.CancelamentoReserva, valor, descricao, idempotencyKey, transacaoReservaId);
            _transacoes.Add(transacao);

            AdicionarEvento(new TransacaoCriadaEvent(transacao.Id, Id, TipoOperacao.CancelamentoReserva, valor, descricao, idempotencyKey, transacaoReservaId));
            AdicionarEvento(new SaldoAtualizadoEvent(Id, saldoDisponivelAnterior, SaldoDisponivel, saldoReservadoAnterior, SaldoReservado));

            AtualizarTimestamp();
        }

        public void Estornar(decimal valor, Guid transacaoOriginalId, string descricao, Guid idempotencyKey)
        {
            ValidarOperacao(valor);

            var saldoDisponivelAnterior = SaldoDisponivel;
            var saldoReservadoAnterior = SaldoReservado;

            SaldoDisponivel += valor;

            var transacao = Transacao.Criar(Id, TipoOperacao.Estorno, valor, descricao, idempotencyKey, transacaoOriginalId);
            _transacoes.Add(transacao);

            AdicionarEvento(new TransacaoCriadaEvent(transacao.Id, Id, TipoOperacao.Estorno, valor, descricao, idempotencyKey, transacaoOriginalId));
            AdicionarEvento(new SaldoAtualizadoEvent(Id, saldoDisponivelAnterior, SaldoDisponivel, saldoReservadoAnterior, SaldoReservado));

            AtualizarTimestamp();
        }

        public void Bloquear()
        {
            var statusAnterior = Status;
            Status = StatusConta.Bloqueada;

            AdicionarEvento(new ContaBloqueadaEvent(Id, Numero, "Conta bloqueada", statusAnterior));

            AtualizarTimestamp();
        }

        public void Desbloquear()
        {
            Status = StatusConta.Ativa;
            AtualizarTimestamp();
        }

        private void ValidarOperacao(decimal valor)
        {
            if (Status != StatusConta.Ativa)
                throw new ContaBloqueadaException($"Conta {Numero} não está ativa. Status: {Status}");

            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero", nameof(valor));
        }
    }
}
