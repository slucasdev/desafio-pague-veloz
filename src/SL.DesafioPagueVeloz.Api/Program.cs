using SL.DesafioPagueVeloz.Api.Extensions;
using SL.DesafioPagueVeloz.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Controllers & AutoMapper
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(SL.DesafioPagueVeloz.Application.Mappings.MappingProfile));

// Custom Extensions
builder.Services.AddSwaggerConfiguration();
builder.Services.AddDatabaseConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddMediatRConfiguration();
builder.Services.AddValidationConfiguration();
builder.Services.AddDependencyInjection();
builder.Services.AddHealthCheckConfiguration();
builder.Services.AddRateLimitingConfiguration();

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

// Middlewares
app.UseCustomMiddleware();

// Swagger
app.UseSwaggerConfiguration();

// Apply Migrations
await app.ApplyMigrationsAsync();

// Endpoints
app.MapControllers();
app.MapHealthCheckEndpoints();

app.Run();

// Adicionado partial class para permitir testes de integração (Recomendação oficial da Microsoft)
public partial class Program { }