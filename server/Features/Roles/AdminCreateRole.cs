using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Roles;

public static class AdminCreateRole
{
  public class Command : AdminAuthRequest<ApiResult<AdminCreateRoleResponse>>
  {
    public string Name { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.Name).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminCreateRoleResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminCreateRoleResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var role = new Role
      {
        Name = request.Name,
      };

      try
      {
        _dbContext.Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminCreateRoleResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminCreateRoleResponse
          {
            RoleId = role.RoleId,
            Name = role.Name,
          },
          SuccessMessage = Message.RoleCreatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.RoleCreationUnexpectedError);

        return new ApiResult<AdminCreateRoleResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.RoleCreationUnexpectedError)
        };
      }
    }
  }
}

public class AdminCreateRoleEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
         "api/admin/roles",
         async (AdminCreateRoleRequest request, ISender sender) =>
         {
           var command = new AdminCreateRole.Command
           {
             Name = request.Name,
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminCreateRole");
  }
}

public record AdminCreateRoleRequest
{
  public string Name { get; set; } = string.Empty;
}

public class AdminCreateRoleResponse
{
  public Guid RoleId { get; set; }
  public string Name { get; set; } = string.Empty;
}