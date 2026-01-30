using Microsoft.EntityFrameworkCore;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Context;

namespace SL.DesafioPagueVeloz.Api.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(60);
                        sqlOptions.MigrationsAssembly("SL.DesafioPagueVeloz.Infrastructure");
                    }));
        }

        return services;
    }

    public static async Task<WebApplication> ApplyMigrationsAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                await dbContext.Database.MigrateAsync();
                app.Logger.LogInformation("Migrations aplicadas com sucesso");
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Erro ao aplicar migrations");
            }
        }

        return app;
    }
}