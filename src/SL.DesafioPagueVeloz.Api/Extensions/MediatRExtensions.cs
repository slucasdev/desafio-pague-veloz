using System.Reflection;
using SL.DesafioPagueVeloz.Application.Behaviors;

namespace SL.DesafioPagueVeloz.Api.Extensions;

public static class MediatRExtensions
{
    public static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            // Handlers
            cfg.RegisterServicesFromAssembly(Assembly.Load("SL.DesafioPagueVeloz.Application"));

            // Behaviors
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        return services;
    }
}