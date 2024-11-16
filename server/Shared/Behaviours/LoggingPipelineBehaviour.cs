using MediatR;

namespace RSPWebAPI.Shared.Behaviours;

internal sealed class LoggingPipelineBehaviour<TRequest, TResponse>
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> _logger;

  public LoggingPipelineBehaviour(ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger)
  {
    _logger = logger;
  }

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    var requestName = request.ToString();
    _logger.LogInformation("Handling request {RequestName}", requestName);

    try
    {
      var response = await next();

      if (response is ApiResult result && result.IsSuccess)
      {
        _logger.LogInformation("Completed request {RequestName} successfully", requestName);
      }
      else
      {
        _logger.LogError("Completed request {RequestName} with error", requestName);
      }

      return response;
    }
    catch
    {
      _logger.LogError("Exception occurred while handling request {RequestName}", requestName);
      throw; // Rethrow to be caught by the exception handling pipeline
    }
  }
}
