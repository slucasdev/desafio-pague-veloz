using AutoMapper;
using FluentAssertions;
using Moq;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.DTOs;
using SL.DesafioPagueVeloz.Application.Handlers;
using SL.DesafioPagueVeloz.Application.Mappings;
using SL.DesafioPagueVeloz.Application.Responses;
using SL.DesafioPagueVeloz.Application.Tests.Fixtures;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Tests.Handlers
{
    public class CreditarContaCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IContaRepository> _contaRepositoryMock;
        private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
        private readonly IMapper _mapper;
        private readonly CreditarContaCommandHandler _handler;

        public CreditarContaCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _contaRepositoryMock = new Mock<IContaRepository>();
            _transacaoRepositoryMock = new Mock<ITransacaoRepository>();

            // Setup repositories no UnitOfWork
            _unitOfWorkMock.Setup(u => u.Contas).Returns(_contaRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepositoryMock.Object);

            // Configurar AutoMapper
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _handler = new CreditarContaCommandHandler(
                _unitOfWorkMock.Object,
                MockLogger.Create<CreditarContaCommandHandler>(),
                _mapper);
        }

        [Fact]
        public async Task Handle_ComContaValida_DeveCreditar()
        {
            // Arrange
            var contaId = Guid.NewGuid();
            var conta = Conta.Criar(Guid.NewGuid(), "00001-5", 1000);

            var command = new CreditarContaCommand
            {
                ContaId = contaId,
                Valor = 500m,
                Descricao = "Depósito",
                IdempotencyKey = Guid.NewGuid()
            };

            // Mock: Não há transação existente (idempotência)
            _transacaoRepositoryMock
                .Setup(r => r.ObterPorIdempotencyKeyAsync(command.IdempotencyKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Transacao?)null);

            // Mock: ExecuteInTransactionAsync executa a função e retorna o resultado
            _unitOfWorkMock
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<OperationResult<TransacaoDTO>>>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<Task<OperationResult<TransacaoDTO>>>, CancellationToken>(async (func, ct) => await func());

            // Mock: Retorna a conta com lock
            _contaRepositoryMock
                .Setup(r => r.ObterComLockAsync(contaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(conta);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Valor.Should().Be(500m);

            // Verificar que Atualizar foi chamado
            _contaRepositoryMock.Verify(r => r.Atualizar(It.IsAny<Conta>()), Times.Once);

            // Verificar que CommitAsync foi chamado (2x: conta + transação processada)
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.AtLeast(1));
        }

        [Fact]
        public async Task Handle_ComContaInexistente_DeveRetornarFalha()
        {
            // Arrange
            var command = new CreditarContaCommand
            {
                ContaId = Guid.NewGuid(),
                Valor = 500m,
                Descricao = "Depósito",
                IdempotencyKey = Guid.NewGuid()
            };

            // Mock: Não há transação existente
            _transacaoRepositoryMock
                .Setup(r => r.ObterPorIdempotencyKeyAsync(command.IdempotencyKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Transacao?)null);

            // Mock: ExecuteInTransactionAsync executa a função
            _unitOfWorkMock
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<OperationResult<TransacaoDTO>>>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<Task<OperationResult<TransacaoDTO>>>, CancellationToken>(async (func, ct) => await func());

            // Mock: Conta não encontrada
            _contaRepositoryMock
                .Setup(r => r.ObterComLockAsync(command.ContaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Conta?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("não encontrada");

            // Verificar que Atualizar NÃO foi chamado
            _contaRepositoryMock.Verify(r => r.Atualizar(It.IsAny<Conta>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ComIdempotencyKeyExistente_DeveRetornarTransacaoExistente()
        {
            // Arrange
            var transacaoExistente = Transacao.Criar(
                Guid.NewGuid(),
                Domain.Enums.TipoOperacao.Credito,
                500m,
                "Depósito",
                Guid.NewGuid());
            transacaoExistente.MarcarComoProcessada();

            var command = new CreditarContaCommand
            {
                ContaId = Guid.NewGuid(),
                Valor = 500m,
                Descricao = "Depósito",
                IdempotencyKey = transacaoExistente.IdempotencyKey
            };

            // Mock: Retorna transação existente (idempotência)
            _transacaoRepositoryMock
                .Setup(r => r.ObterPorIdempotencyKeyAsync(command.IdempotencyKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transacaoExistente);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("já processada");
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(transacaoExistente.Id);

            // Verificar que NÃO tentou processar novamente
            _contaRepositoryMock.Verify(r => r.ObterComLockAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}