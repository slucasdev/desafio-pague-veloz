using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SL.DesafioPagueVeloz.Domain.Entities;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedNever();

            builder.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255);

            // Value Object: Documento
            builder.OwnsOne(c => c.Documento, documento =>
            {
                documento.Property(d => d.Numero)
                    .HasColumnName("Documento")
                    .IsRequired()
                    .HasMaxLength(14)
                    .HasColumnType("varchar(14)");

                documento.Property(d => d.Tipo)
                    .HasColumnName("TipoDocumento")
                    .IsRequired()
                    .HasConversion<int>();

                documento.HasIndex(d => d.Numero)
                    .HasDatabaseName("IX_Clientes_Documento")
                    .IsUnique();
            });

            builder.Property(c => c.Ativo)
                .IsRequired();

            builder.Property(c => c.CriadoEm)
                .IsRequired();

            builder.Property(c => c.AtualizadoEm);

            // Relacionamentos
            builder.HasMany(c => c.Contas)
                .WithOne(co => co.Cliente)
                .HasForeignKey(co => co.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_Clientes_Email");

            builder.HasIndex(c => c.Ativo)
                .HasDatabaseName("IX_Clientes_Ativo");

            // Ignorar eventos de domínio
            builder.Ignore(c => c.DomainEvents);
        }
    }
}
