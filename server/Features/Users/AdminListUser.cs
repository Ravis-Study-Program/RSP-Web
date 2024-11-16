using System.ComponentModel.DataAnnotations;
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

public static class AdminListUser
{
  public class Command : AdminAuthRequest<ApiResult<AdminListUserResponse>> { }

  public class Handler : IRequestHandler<Command, ApiResult<AdminListUserResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminListUserResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var users = await _dbContext.Users.ToListAsync(cancellationToken);

        return new ApiResult<AdminListUserResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.UserListSuccessfully,
          ResponseBody = new AdminListUserResponse { Users = users },
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.UserListUnexpectedError);

        return new ApiResult<AdminListUserResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.UserListUnexpectedError),
        };
      }
    }
  }
}

public class AdminListUserEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
        "api/admin/users",
        async (ISender sender) =>
        {
          var command = new AdminListUser.Command();
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminListUser");
  }
}

public class AdminListUserResponse
{
  [Required]
  public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
}
