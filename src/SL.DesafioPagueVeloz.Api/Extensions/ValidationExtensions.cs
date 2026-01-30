using FluentValidation;
using System.Reflection;

namespace SL.DesafioPagueVeloz.Api.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.Load("SL.DesafioPagueVeloz.Application"));
        return services;
    }
}