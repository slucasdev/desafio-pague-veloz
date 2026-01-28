using FluentAssertions;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Enums;
using SL.DesafioPagueVeloz.Domain.Exceptions;

namespace SL.DesafioPagueVeloz.Domain.Tests.Entities
{
    public class ContaTests
    {
        [Fact]
        public void Criar_ComDadosValidos_DeveCriarConta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var numero = "00001-5";
            var limiteCredito = 1000m;

            // Act
            var conta = Conta.Criar(clienteId, numero, limiteCredito);

            // Assert
            conta.Should().NotBeNull();
            conta.Id.Should().NotBeEmpty();
            conta.ClienteId.Should().Be(clienteId);
            conta.Numero.Should().Be(numero);
            conta.LimiteCredito.Should().Be(limiteCredito);
            conta.SaldoDisponivel.Should().Be(0);
            conta.SaldoReservado.Should().Be(0);
            conta.Status.Should().Be(StatusConta.Ativa);
        }

        [Fact]
        public void Creditar_ComValorValido_DeveAumentarSaldo()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            var valorCredito = 500m;

            // Act
            conta.Creditar(valorCredito, "Depósito", Guid.NewGuid());

            // Assert
            conta.SaldoDisponivel.Should().Be(500m);
            conta.Transacoes.Should().HaveCount(1);
            conta.Transacoes.First().Tipo.Should().Be(TipoOperacao.Credito);
            conta.Transacoes.First().Valor.Should().Be(valorCredito);
        }

        [Fact]
        public void Creditar_DeveGerarEventosDeDominio()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.LimparEventos();

            // Act
            conta.Creditar(500m, "Depósito", Guid.NewGuid());

            // Assert
            conta.DomainEvents.Should().HaveCountGreaterThanOrEqualTo(2);
            conta.DomainEvents.Should().Contain(e => e.TipoEvento == "TransacaoCriadaEvent");
            conta.DomainEvents.Should().Contain(e => e.TipoEvento == "SaldoAtualizadoEvent");
        }

        [Fact]
        public void Debitar_ComSaldoSuficiente_DeveDiminuirSaldo()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Creditar(500m, "Depósito", Guid.NewGuid());

            // Act
            conta.Debitar(200m, "Pagamento", Guid.NewGuid());

            // Assert
            conta.SaldoDisponivel.Should().Be(300m); // 500 - 200
            conta.Transacoes.Should().HaveCount(2);
            conta.Transacoes.Last().Tipo.Should().Be(TipoOperacao.Debito);
        }

        [Fact]
        public void Debitar_ComSaldoInsuficiente_SemLimite_DeveLancarException()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 0);
            conta.Creditar(100m, "Depósito", Guid.NewGuid());
            // Saldo: 100, Limite: 0, SaldoTotal: 100

            // Act
            Action act = () => conta.Debitar(200m, "Pagamento", Guid.NewGuid());

            // Assert
            act.Should().Throw<SaldoInsuficienteException>()
                .WithMessage("Saldo insuficiente*");
        }

        [Fact]
        public void Debitar_UsandoLimiteCredito_DevePermitirOperacao()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            // Saldo: 0, Limite: 1000, SaldoTotal: 1000

            // Act
            conta.Debitar(500m, "Pagamento", Guid.NewGuid());

            // Assert
            conta.SaldoDisponivel.Should().Be(-500m);
            conta.SaldoTotal.Should().Be(500m); // 1000 - 500 = 500
        }

        [Fact]
        public void Debitar_ExcedendoLimiteCredito_DeveLancarException()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            // Saldo: 0, Limite: 1000, SaldoTotal: 1000

            // Act
            Action act = () => conta.Debitar(1500m, "Pagamento", Guid.NewGuid());

            // Assert
            act.Should().Throw<SaldoInsuficienteException>();
        }

        [Fact]
        public void Reservar_ComSaldoSuficiente_DeveReservarValor()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Creditar(500m, "Depósito", Guid.NewGuid());

            // Act
            conta.Reservar(200m, "Reserva cartão", Guid.NewGuid());

            // Assert
            conta.SaldoDisponivel.Should().Be(300m); // 500 - 200
            conta.SaldoReservado.Should().Be(200m);
            conta.Transacoes.Last().Tipo.Should().Be(TipoOperacao.Reserva);
        }

        [Fact]
        public void Reservar_ComSaldoInsuficiente_SemLimite_DeveLancarException()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 0);
            conta.Creditar(100m, "Depósito", Guid.NewGuid());
            // Saldo: 100, Limite: 0, SaldoTotal: 100

            // Act
            Action act = () => conta.Reservar(200m, "Reserva", Guid.NewGuid());

            // Assert
            act.Should().Throw<SaldoInsuficienteException>();
        }

        [Fact]
        public void Capturar_ComReservaExistente_DeveCapturarValor()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Creditar(500m, "Depósito", Guid.NewGuid());
            conta.Reservar(200m, "Reserva", Guid.NewGuid());
            var transacaoReservaId = conta.Transacoes.Last().Id;

            // Act
            conta.Capturar(200m, transacaoReservaId, "Captura", Guid.NewGuid());

            // Assert
            conta.SaldoDisponivel.Should().Be(300m); // Continua 300
            conta.SaldoReservado.Should().Be(0m); // 200 foi capturado
            conta.Transacoes.Last().Tipo.Should().Be(TipoOperacao.Captura);
        }

        [Fact]
        public void CancelarReserva_ComReservaExistente_DeveDevolverValor()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Creditar(500m, "Depósito", Guid.NewGuid());
            conta.Reservar(200m, "Reserva", Guid.NewGuid());
            var transacaoReservaId = conta.Transacoes.Last().Id;

            // Act
            conta.CancelarReserva(200m, transacaoReservaId, "Cancelamento", Guid.NewGuid());

            // Assert
            conta.SaldoDisponivel.Should().Be(500m); // Voltou para 500
            conta.SaldoReservado.Should().Be(0m);
            conta.Transacoes.Last().Tipo.Should().Be(TipoOperacao.CancelamentoReserva);
        }

        [Fact]
        public void Estornar_DeveReverterOperacao()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Creditar(500m, "Depósito", Guid.NewGuid());
            conta.Debitar(200m, "Pagamento", Guid.NewGuid());
            var transacaoDebitoId = conta.Transacoes.Last().Id;
            // Saldo: 300

            // Act
            conta.Estornar(200m, transacaoDebitoId, "Estorno de pagamento", Guid.NewGuid());

            // Assert
            conta.SaldoDisponivel.Should().Be(500m); // Voltou para 500
            conta.Transacoes.Last().Tipo.Should().Be(TipoOperacao.Estorno);
        }

        [Fact]
        public void Bloquear_DeveAlterarStatusParaBloqueada()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);

            // Act
            conta.Bloquear("Teste de bloqueio");

            // Assert
            conta.Status.Should().Be(StatusConta.Bloqueada);
        }

        [Fact]
        public void Debitar_ComContaBloqueada_DeveLancarException()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Creditar(500m, "Depósito", Guid.NewGuid());
            conta.Bloquear("Teste de bloqueio");

            // Act
            Action act = () => conta.Debitar(100m, "Pagamento", Guid.NewGuid());

            // Assert
            act.Should().Throw<ContaBloqueadaException>()
                .WithMessage("*bloqueada*");
        }

        [Fact]
        public void Creditar_ComContaBloqueada_DeveLancarException()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Bloquear("Teste de bloqueio");

            // Act
            Action act = () => conta.Creditar(500m, "Depósito", Guid.NewGuid());

            // Assert
            act.Should().Throw<ContaBloqueadaException>();
        }

        [Fact]
        public void Desbloquear_DeveAlterarStatusParaAtiva()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Bloquear("Teste de bloqueio");

            // Act
            conta.Desbloquear();

            // Assert
            conta.Status.Should().Be(StatusConta.Ativa);
        }

        [Fact]
        public void SaldoTotal_DeveConsiderarSaldoDisponivelELimite()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Creditar(500m, "Depósito", Guid.NewGuid());

            // Act
            var saldoTotal = conta.SaldoTotal;

            // Assert
            saldoTotal.Should().Be(1500m); // 500 + 1000 (limite)
        }

        [Fact]
        public void MultiplaOperacoes_DeveManterConsistenciaDeSaldo()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);

            // Act & Assert
            conta.Creditar(1000m, "Depósito 1", Guid.NewGuid());
            conta.SaldoDisponivel.Should().Be(1000m);

            conta.Debitar(300m, "Pagamento 1", Guid.NewGuid());
            conta.SaldoDisponivel.Should().Be(700m);

            conta.Reservar(200m, "Reserva 1", Guid.NewGuid());
            conta.SaldoDisponivel.Should().Be(500m);
            conta.SaldoReservado.Should().Be(200m);

            var reservaId = conta.Transacoes.Last().Id;
            conta.Capturar(200m, reservaId, "Captura 1", Guid.NewGuid());
            conta.SaldoDisponivel.Should().Be(500m);
            conta.SaldoReservado.Should().Be(0m);

            conta.Transacoes.Should().HaveCount(4);
        }

        [Fact]
        public void SaldoTotal_ComSaldoNegativo_DeveDescontarDoLimite()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.Debitar(300m, "Usando limite", Guid.NewGuid());

            // Act
            var saldoTotal = conta.SaldoTotal;

            // Assert
            conta.SaldoDisponivel.Should().Be(-300m);
            saldoTotal.Should().Be(700m); // 1000 - 300 = 700
        }

        [Fact]
        public void Bloquear_DeveGerarEventoDeDominio()
        {
            // Arrange
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);
            conta.LimparEventos();

            // Act
            conta.Bloquear("Teste de bloqueio");

            // Assert
            conta.DomainEvents.Should().NotBeEmpty();
            conta.DomainEvents.Should().Contain(e => e.TipoEvento == "ContaBloqueadaEvent");
        }
    }
}