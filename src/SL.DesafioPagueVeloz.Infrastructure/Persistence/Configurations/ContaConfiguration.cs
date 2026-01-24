using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SL.DesafioPagueVeloz.Domain.Entities;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Configurations
{
    public class ContaConfiguration : IEntityTypeConfiguration<Conta>
    {
        public void Configure(EntityTypeBuilder<Conta> builder)
        {
            builder.ToTable("Contas");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedNever();

            builder.Property(c => c.ClienteId)
                .IsRequired();

            builder.Property(c => c.Numero)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.SaldoDisponivel)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.SaldoReservado)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.LimiteCredito)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(c => c.CriadoEm)
                .IsRequired();

            builder.Property(c => c.AtualizadoEm);

            // Relacionamentos
            builder.HasOne(c => c.Cliente)
                .WithMany(cl => cl.Contas)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Transacoes)
                .WithOne(t => t.Conta)
                .HasForeignKey(t => t.ContaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(c => c.Numero)
                .IsUnique()
                .HasDatabaseName("IX_Contas_Numero");

            builder.HasIndex(c => c.ClienteId)
                .HasDatabaseName("IX_Contas_ClienteId");

            builder.HasIndex(c => c.Status)
                .HasDatabaseName("IX_Contas_Status");

            // Ignorar eventos de domínio
            builder.Ignore(c => c.DomainEvents);
        }
    }
}
