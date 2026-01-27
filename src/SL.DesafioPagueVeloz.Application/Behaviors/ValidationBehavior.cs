using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Application.Responses;

namespace SL.DesafioPagueVeloz.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        public ValidationBehavior(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                var errors = failures.Select(f => f.ErrorMessage).ToList();

                _logger.LogWarning("Validação falhou para {RequestType}: {Errors}",
                    typeof(TRequest).Name,
                    string.Join(", ", errors));

                // Se TResponse é um OperationResult, retornar erro de validação
                if (typeof(TResponse).IsGenericType &&
                    typeof(TResponse).GetGenericTypeDefinition() == typeof(OperationResult<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var failureMethod = typeof(OperationResult<>)
                        .MakeGenericType(resultType)
                        .GetMethod(nameof(OperationResult<object>.FailureResult),
                            new[] { typeof(string), typeof(List<string>) });

                    if (failureMethod != null)
                    {
                        var result = failureMethod.Invoke(null, new object[]
                        {
                            "Erro de validação",
                            errors
                        });

                        return (TResponse)result!;
                    }
                }

                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
