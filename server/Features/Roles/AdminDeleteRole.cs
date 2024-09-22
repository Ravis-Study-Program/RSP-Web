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

public static class AdminDeleteRole
{
  public class Command : AdminAuthRequest<ApiResult<AdminDeleteRoleResponse>>
  {
    public Guid RoleId { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.RoleId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminDeleteRoleResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminDeleteRoleResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingRole = await _dbContext
                                 .Roles.FirstOrDefaultAsync(u => u.RoleId == request.RoleId, cancellationToken);
      if (existingRole == null)
      {
        return new ApiResult<AdminDeleteRoleResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.RoleDoesNotExists)
        };
      }

      try
      {
        _dbContext.Remove(existingRole);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminDeleteRoleResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.RoleDeletedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.RoleDeletionUnexpectedError);

        return new ApiResult<AdminDeleteRoleResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.RoleDeletionUnexpectedError)
        };
      }
    }
  }
}

public class AdminDeleteRoleEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete(
         "api/admin/roles",
         async (Guid roleId, ISender sender) =>
         {
           var command = new AdminDeleteRole.Command
           {
             RoleId = roleId
           };
           var response = await sender.Send(command);

           return Results.Json(response, statusCode: (int)response.StatusCode);
         }
       )
       .WithName("AdminDeleteRole");
  }
}

public class AdminDeleteRoleResponse
{
}