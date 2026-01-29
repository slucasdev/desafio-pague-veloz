using AutoMapper;
using FluentAssertions;
using Moq;
using SL.DesafioPagueVeloz.Application.Commands;
using SL.DesafioPagueVeloz.Application.Handlers;
using SL.DesafioPagueVeloz.Application.Mappings;
using SL.DesafioPagueVeloz.Application.Tests.Fixtures;
using SL.DesafioPagueVeloz.Domain.Entities;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Tests.Handlers
{
    public class CriarClienteCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly IMapper _mapper;
        private readonly CriarClienteCommandHandler _handler;

        public CriarClienteCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _clienteRepositoryMock = new Mock<IClienteRepository>();

            _unitOfWorkMock.Setup(u => u.Clientes).Returns(_clienteRepositoryMock.Object);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _handler = new CriarClienteCommandHandler(
                _unitOfWorkMock.Object,
                MockLogger.Create<CriarClienteCommandHandler>(),
                _mapper);
        }

        [Fact]
        public async Task Handle_ComDadosValidos_DeveCriarCliente()
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = "João Silva",
                Documento = "12345678901",
                Email = "joao@email.com"
            };

            // Mock: Documento não existe
            _clienteRepositoryMock
                .Setup(r => r.ExisteDocumentoAsync(command.Documento, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Nome.Should().Be(command.Nome);
            result.Data.Email.Should().Be(command.Email);
            result.Data.Documento.Should().Be(command.Documento);
            result.Data.Ativo.Should().BeTrue();

            // Verificar que AdicionarAsync foi chamado
            _clienteRepositoryMock.Verify(
                r => r.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verificar que CommitAsync foi chamado
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ComDocumentoJaExistente_DeveRetornarFalha()
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = "João Silva",
                Documento = "12345678901",
                Email = "joao@email.com"
            };

            // Mock: Documento já existe
            _clienteRepositoryMock
                .Setup(r => r.ExisteDocumentoAsync(command.Documento, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("já cadastrado");

            // Verificar que NÃO tentou adicionar
            _clienteRepositoryMock.Verify(
                r => r.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ComDocumentoInvalido_DeveLancarException()
        {
            // Arrange
            var command = new CriarClienteCommand
            {
                Nome = "João Silva",
                Documento = "123", // Documento inválido
                Email = "joao@email.com"
            };

            // Mock: Documento não existe
            _clienteRepositoryMock
                .Setup(r => r.ExisteDocumentoAsync(command.Documento, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Erro ao criar cliente");
        }
    }
}