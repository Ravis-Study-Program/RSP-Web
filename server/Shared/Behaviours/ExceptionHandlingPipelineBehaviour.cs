using MediatR;

namespace RSPWebAPI.Shared.Behaviours
{
    internal sealed class ExceptionHandlingPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        private readonly ILogger<ExceptionHandlingPipelineBehaviour<TRequest, TResponse>> _logger;

        public ExceptionHandlingPipelineBehaviour(ILogger<ExceptionHandlingPipelineBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;
                _logger.LogError(ex, "Unhandled exception for request {RequestName}", requestName);

                var error = new Error("UnhandledException", "An unexpected error occurred.");

                // Handle the non-generic Result case
                if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure(error);
                }

                var genericArgumentType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethod(nameof(Result.Failure), new Type[] { typeof(Error) })?
                    .MakeGenericMethod(genericArgumentType);

                var failureResult = failureMethod?.Invoke(null, new object[] { error });
                return (TResponse)failureResult!;
            }
        }
    }
}
