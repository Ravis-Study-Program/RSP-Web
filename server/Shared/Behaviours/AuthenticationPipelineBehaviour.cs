using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;

namespace RSPWebAPI.Shared.Behaviours;

public class AuthenticationPipelineBehavior<TRequest, TResponse>
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : AuthRequest<TResponse>
  where TResponse : ApiResult, new()
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<AuthenticationPipelineBehavior<TRequest, TResponse>> _logger;

  public AuthenticationPipelineBehavior(IHttpContextAccessor httpContextAccessor,
    ILogger<AuthenticationPipelineBehavior<TRequest, TResponse>> logger)
  {
    _httpContextAccessor = httpContextAccessor;
    _logger = logger;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    var context = _httpContextAccessor.HttpContext;
    if (context?.User?.Identity?.IsAuthenticated != true)
    {
      return new TResponse
      {
        StatusCode = HttpStatusCode.Unauthorized,
        Error = new ApiError("You don't have the required authorization to access the resource.")
      };
    }

    return await next();
  }
}

public class AdminAuthenticationPipelineBehaviour<TRequest, TResponse>
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : AdminAuthRequest<TResponse>
  where TResponse : ApiResult, new()
{
  private readonly ApplicationDbContext _dbContext;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<AdminAuthenticationPipelineBehaviour<TRequest, TResponse>> _logger;

  public AdminAuthenticationPipelineBehaviour(IHttpContextAccessor httpContextAccessor,
    ILogger<AdminAuthenticationPipelineBehaviour<TRequest, TResponse>> logger,
    ApplicationDbContext dbContext)
  {
    _httpContextAccessor = httpContextAccessor;
    _logger = logger;
    _dbContext = dbContext;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    var context = _httpContextAccessor.HttpContext;
    var user = context?.User?.Identity;
    var name = user?.Name;

    var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == name && u.IsAdmin, cancellationToken);
    if (foundUser == null)
    {
      return new TResponse
      {
        StatusCode = HttpStatusCode.Unauthorized,
        Error = new ApiError("You don't have the required authorization to access the resource.")
      };
    }

    return await next();
  }
}

public class AuthRequest<TResponse> : IRequest<TResponse>
{
}

public class AdminAuthRequest<TResponse> : AuthRequest<TResponse>
{
}