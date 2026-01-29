using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SL.DesafioPagueVeloz.Domain.Entities;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .ValueGeneratedNever();

            builder.Property(o => o.TipoEvento)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(o => o.ConteudoJson)
                .IsRequired();

            builder.Property(o => o.Processado)
                .IsRequired();

            builder.Property(o => o.ProcessadoEm);

            builder.Property(o => o.TentativasProcessamento)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(o => o.ErroProcessamento)
                .HasMaxLength(2000);

            builder.Property(o => o.ProximaTentativaEm);

            builder.Property(o => o.CriadoEm)
                .IsRequired();

            builder.Property(o => o.AtualizadoEm);

            // Índices para performance do background worker
            builder.HasIndex(o => o.Processado)
                .HasDatabaseName("IX_OutboxMessages_Processado");

            builder.HasIndex(o => new { o.Processado, o.CriadoEm })
                .HasDatabaseName("IX_OutboxMessages_Processado_CriadoEm");

            builder.HasIndex(o => o.ProximaTentativaEm)
                .HasDatabaseName("IX_OutboxMessages_ProximaTentativaEm")
                .HasFilter("[ProximaTentativaEm] IS NOT NULL");

            builder.HasIndex(o => new { o.Processado, o.TentativasProcessamento })
                .HasDatabaseName("IX_OutboxMessages_Processado_Tentativas");

            // Ignorar eventos de domínio
            builder.Ignore(o => o.DomainEvents);
        }
    }
}
