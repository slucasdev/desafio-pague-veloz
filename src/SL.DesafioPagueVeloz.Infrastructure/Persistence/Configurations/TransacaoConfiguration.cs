using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SL.DesafioPagueVeloz.Domain.Entities;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Configurations
{
    public class TransacaoConfiguration : IEntityTypeConfiguration<Transacao>
    {
        public void Configure(EntityTypeBuilder<Transacao> builder)
        {
            builder.ToTable("Transacoes");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .ValueGeneratedNever();

            builder.Property(t => t.ContaId)
                .IsRequired();

            builder.Property(t => t.Tipo)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.Valor)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.Descricao)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.IdempotencyKey)
                .IsRequired();

            builder.Property(t => t.TransacaoOrigemId);

            builder.Property(t => t.ProcessadoEm);

            builder.Property(t => t.MotivoFalha)
                .HasMaxLength(1000);

            builder.Property(t => t.CriadoEm)
                .IsRequired();

            builder.Property(t => t.AtualizadoEm);

            // Relacionamentos
            builder.HasOne(t => t.Conta)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.ContaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(t => t.IdempotencyKey)
                .IsUnique()
                .HasDatabaseName("IX_Transacoes_IdempotencyKey");

            builder.HasIndex(t => t.ContaId)
                .HasDatabaseName("IX_Transacoes_ContaId");

            builder.HasIndex(t => t.Status)
                .HasDatabaseName("IX_Transacoes_Status");

            builder.HasIndex(t => t.CriadoEm)
                .HasDatabaseName("IX_Transacoes_CriadoEm");

            builder.HasIndex(t => new { t.ContaId, t.CriadoEm })
                .HasDatabaseName("IX_Transacoes_ContaId_CriadoEm");

            builder.HasIndex(t => t.TransacaoOrigemId)
                .HasDatabaseName("IX_Transacoes_TransacaoOrigemId");

            // Ignorar eventos de domínio
            builder.Ignore(t => t.DomainEvents);
        }
    }
}
