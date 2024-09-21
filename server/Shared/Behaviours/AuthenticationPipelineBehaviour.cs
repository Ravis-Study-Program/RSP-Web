using System.Security.Claims;
using MediatR;

namespace RSPWebAPI.Shared.Behaviours;

public class AuthenticationPipelineBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : AuthRequest<TResponse>
    where TResponse : Result
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthenticationPipelineBehavior<TRequest, TResponse>> _logger;

    public AuthenticationPipelineBehavior(IHttpContextAccessor httpContextAccessor,
        ILogger<AuthenticationPipelineBehavior<TRequest, TResponse>> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext; 
        _logger.LogInformation(context?.User?.Identity?.IsAuthenticated.ToString());
        if (context?.User?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Testst");
        }

        return await next();
    }
}

public class AuthRequest<TResponse> : IRequest<TResponse>
{

}