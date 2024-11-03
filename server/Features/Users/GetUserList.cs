using System.Net;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Users;

public static class GetUserList
{
  public class Command : AuthRequest<ApiResult<GetUserListResponse>>
  {
  }

  public class Handler : IRequestHandler<Command, ApiResult<GetUserListResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetUserListResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var users = await _dbContext.Users.ToListAsync(cancellationToken);

        return new ApiResult<GetUserListResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.UserListSuccessfully,
          ResponseBody = new GetUserListResponse
          {
            Users = users
          }
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.UserListUnexpectedError);

        return new ApiResult<GetUserListResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.UserListUnexpectedError)
        };
      }
    }
  }
}

public class GetUserListEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/users-list",
         async (ISender sender) =>
         {
           var command = new GetUserList.Command();
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("GetUserList");
  }
}

public class GetUserListResponse
{
  public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
}
