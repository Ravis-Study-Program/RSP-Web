using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Roles;

public static class AdminUpdateRole
{
  public class Command : AdminAuthRequest<ApiResult<AdminUpdateRoleResponse>>
  {
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.RoleId).NotEmpty();
      RuleFor(c => c.Name).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminUpdateRoleResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminUpdateRoleResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingRole = await _dbContext
                                 .Roles.FirstOrDefaultAsync(u => u.RoleId == request.RoleId, cancellationToken);
      if (existingRole == null)
      {
        return new ApiResult<AdminUpdateRoleResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.RoleDoesNotExists)
        };
      }

      existingRole.Name = request.Name;

      try
      {
        _dbContext.Update(existingRole);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminUpdateRoleResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminUpdateRoleResponse
          {
            RoleId = existingRole.RoleId,
            Name = existingRole.Name,
          },
          SuccessMessage = Message.RoleUpdatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.RoleUpdateUnexpectedError);

        return new ApiResult<AdminUpdateRoleResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.RoleUpdateUnexpectedError)
        };
      }
    }
  }
}

public class AdminUpdateRoleEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPut(
         "api/admin/roles",
         async (AdminUpdateRoleRequest request, ISender sender) =>
         {
           var command = new AdminUpdateRole.Command
           {
             RoleId = request.RoleId,
             Name = request.Name
           };
           var response = await sender.Send(command);

           return Results.Json(response, statusCode: (int)response.StatusCode);
         }
       )
       .WithName("AdminUpdateRole");
  }
}

public record AdminUpdateRoleRequest
{
  public Guid RoleId { get; set; }
  public string Name { get; set; } = string.Empty;
}

public class AdminUpdateRoleResponse
{
  public Guid RoleId { get; set; }
  public string Name { get; set; } = string.Empty;
}