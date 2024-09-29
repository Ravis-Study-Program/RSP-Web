using System.Net;
using Carter;
using MediatR;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Roles;

public static class AdminListRole
{
  public class Command : AdminAuthRequest<ApiResult<AdminListRoleResponse>>
  {
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminListRoleResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminListRoleResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var roles = _dbContext.Roles.ToList();

        return new ApiResult<AdminListRoleResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.RoleListSuccessfully,
          ResponseBody = new AdminListRoleResponse
          {
            Roles = roles
          }
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.RoleListUnexpectedError);

        return new ApiResult<AdminListRoleResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.RoleListUnexpectedError)
        };
      }
    }
  }
}

public class AdminListRoleEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/admin/roles",
         async (ISender sender) =>
         {
           var command = new AdminListRole.Command();
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminListRole");
  }
}

public class AdminListRoleResponse
{
  public ICollection<Role> Roles { get; set; } = new List<Role>();
}
