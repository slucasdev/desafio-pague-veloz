using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SL.DesafioPagueVeloz.Application.Behaviors;
using SL.DesafioPagueVeloz.Application.Interfaces;
using SL.DesafioPagueVeloz.Domain.Interfaces.Repository;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;
using SL.DesafioPagueVeloz.Infrastructure.BackgroundServices;
using SL.DesafioPagueVeloz.Infrastructure.Messaging;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Context;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Repositories;
using SL.DesafioPagueVeloz.Infrastructure.Persistence.Uow;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60);
            sqlOptions.MigrationsAssembly("SL.DesafioPagueVeloz.Infrastructure");
        }
    ));

// MediatR
builder.Services.AddMediatR(cfg =>
{
    // Handlers
    cfg.RegisterServicesFromAssembly(Assembly.Load("SL.DesafioPagueVeloz.Application"));

    // Behaviors
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

// FlientValidation
builder.Services.AddValidatorsFromAssembly(Assembly.Load("SL.DesafioPagueVeloz.Application"));

// Repositories
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

// UOW
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Event Dispatcher
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventPublisher>();

// Background Services
builder.Services.AddHostedService<OutboxProcessorService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Desafio PagueVeloz API")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Apply migrations at startup
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

app.Run();
