using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SL.DesafioPagueVeloz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Documento = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false),
                    TipoDocumento = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ConteudoJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Processado = table.Column<bool>(type: "bit", nullable: false),
                    ProcessadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TentativasProcessamento = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ErroProcessamento = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProximaTentativaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SaldoDisponivel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoReservado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LimiteCredito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransacaoOrigemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProcessadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoFalha = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacoes_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Ativo",
                table: "Clientes",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Documento",
                table: "Clientes",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contas_ClienteId",
                table: "Contas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Contas_Numero",
                table: "Contas",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contas_Status",
                table: "Contas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Processado",
                table: "OutboxMessages",
                column: "Processado");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Processado_CriadoEm",
                table: "OutboxMessages",
                columns: new[] { "Processado", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Processado_Tentativas",
                table: "OutboxMessages",
                columns: new[] { "Processado", "TentativasProcessamento" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProximaTentativaEm",
                table: "OutboxMessages",
                column: "ProximaTentativaEm",
                filter: "[ProximaTentativaEm] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId",
                table: "Transacoes",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId_CriadoEm",
                table: "Transacoes",
                columns: new[] { "ContaId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_CriadoEm",
                table: "Transacoes",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_IdempotencyKey",
                table: "Transacoes",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_Status",
                table: "Transacoes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_TransacaoOrigemId",
                table: "Transacoes",
                column: "TransacaoOrigemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "Transacoes");

            migrationBuilder.DropTable(
                name: "Contas");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
