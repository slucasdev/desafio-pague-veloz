using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SL.DesafioPagueVeloz.Api.Tests.Mocks;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Context;

namespace SL.DesafioPagueVeloz.Api.Tests.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"TestDb_{Guid.NewGuid()}";
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Console.WriteLine($"[Factory] Configurando banco: {_databaseName}");

            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remover background services
                services.RemoveAll<IHostedService>();

                // Substituir ContaRepository por versão compatível com SQLite
                services.Replace(ServiceDescriptor.Scoped<IContaRepository, ContaRepositoryForTests>());

                // Criar conexão SQLite em memória (DEVE ficar aberta durante todos os testes)
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                // Adicionar DbContext com SQLite
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Criar schema do banco (SEM usar migrations)
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // EnsureCreated cria as tabelas baseado nas entidades (ignora migrations)
                // Isso funciona perfeitamente com SQLite para testes
                db.Database.EnsureCreated();

                Console.WriteLine($"[Factory] Banco SQLite criado com sucesso");
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _connection?.Close();
                    _connection?.Dispose();
                }
                catch
                {
                    // Ignorar erros de limpeza
                }
            }

            base.Dispose(disposing);
        }
    }
}