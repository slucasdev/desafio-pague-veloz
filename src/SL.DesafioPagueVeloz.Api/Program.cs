using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SL.DesafioPagueVeloz.Api.Middleware;
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
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Controllers
builder.Services.AddControllers();

// Automapper
builder.Services.AddAutoMapper(typeof(SL.DesafioPagueVeloz.Application.Mappings.MappingProfile));

// Scalar
builder.Services.AddOpenApi();

// Database Context
if (!builder.Environment.IsEnvironment("Testing"))
{
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
}

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

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "sqlserver" })
    .AddCheck("self", () =>
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"),
        tags: new[] { "api" });

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // 1. Política Global (todas as rotas)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Identifica cliente por IP
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,                              // 100 requests
            Window = TimeSpan.FromMinutes(1),               // Por minuto
            SegmentsPerWindow = 6,                          // Divide em 6 segmentos
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 10                                 // 10 requests na fila
        });
    });

    // 2. Política Específica para Transações (mais restritiva)
    options.AddPolicy("transacoes", context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,                               // Apenas 30 transações
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 5
        });
    });

    // 3. Resposta customizada quando exceder limite
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too Many Requests",
                message = $"Rate limit exceeded. Try again in {retryAfter.TotalSeconds} seconds.",
                retryAfter = retryAfter.TotalSeconds
            }, cancellationToken);
        }
        else
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too Many Requests",
                message = "Rate limit exceeded. Please wait before making more requests."
            }, cancellationToken);
        }
    };
});

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

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
// Rate Limiting
app.UseRateLimiter();
app.UseAuthorization();

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

    // Apply migrations at startup
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

app.MapControllers();

// Health Check
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("api")
});
app.MapHealthChecks("/health/details", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                exception = e.Value.Exception?.Message,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.Run();

// Adicionado partial class para permitir testes de integração (Recomendação oficial da Microsoft)
public partial class Program { }