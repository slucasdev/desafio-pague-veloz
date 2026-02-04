using MediatR;
using Microsoft.Extensions.Logging;
using SL.DesafioPagueVeloz.Domain.Interfaces.Uow;

namespace SL.DesafioPagueVeloz.Application.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

        public TransactionBehavior(
            IUnitOfWork unitOfWork,
            ILogger<TransactionBehavior<TRequest, TResponse>> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Apenas aplicar transação para Commands (que modificam dados)
            // Queries não precisam de transação
            if (requestName.EndsWith("Query"))
            {
                return await next();
            }

            _logger.LogInformation("Iniciando transação para {RequestName}", requestName);

            return await _unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                try
                {
                    var response = await next();

                    _logger.LogInformation("Transação para {RequestName} concluída com sucesso", requestName);

                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante transação de {RequestName}", requestName);
                    throw;
                }
            }, cancellationToken);
        }
    }
}
