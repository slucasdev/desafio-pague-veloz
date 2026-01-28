using Microsoft.EntityFrameworkCore;
using SL.DesafioPagueVeloz.Domain.Entities;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SL.DesafioPagueVeloz.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Conta> Contas => Set<Conta>();
        public DbSet<Transacao> Transacoes => Set<Transacao>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configurações globais
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Configurar delete behavior para evitar cascade
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                }

                // Configurar precisão decimal padrão
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(18);
                        property.SetScale(2);
                    }
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Capturar entidades com eventos ANTES de salvar
            var entitiesWithEvents = ChangeTracker
                .Entries<Domain.Common.EntityBase>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            // 2. Converter eventos de domínio em mensagens Outbox
            foreach (var entity in entitiesWithEvents)
            {
                foreach (var domainEvent in entity.DomainEvents)
                {
                    var outboxMessage = OutboxMessage.Criar(
                        domainEvent.TipoEvento,
                        JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions)
                    );

                    await OutboxMessages.AddAsync(outboxMessage, cancellationToken);
                }

                // Limpar eventos da entidade após salvá-los no outbox
                entity.LimparEventos();
            }

            // 3. Salvar tudo em uma única transação (entidades + outbox messages)
            var result = await base.SaveChangesAsync(cancellationToken);

            // Os eventos serão processados de forma assíncrona por um background service
            // que lê a tabela OutboxMessages e publica no message broker

            return result;
        }
    }
}
