using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Users;

public static class AdminDeleteUser
{
  public class Command : AdminAuthRequest<ApiResult<AdminDeleteUserResponse>>
  {
    public string Email { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminDeleteUserResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminDeleteUserResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingUser = await _dbContext.Users.FirstOrDefaultAsync(
        u => u.Email == request.Email,
        cancellationToken
      );
      if (existingUser == null)
      {
        return new ApiResult<AdminDeleteUserResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.UserEmailDoesNotExists),
        };
      }

      try
      {
        _dbContext.Remove(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminDeleteUserResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.UserDeletedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.UserDeletionUnexpectedError);

        return new ApiResult<AdminDeleteUserResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.UserDeletionUnexpectedError),
        };
      }
    }
  }
}

public class AdminDeleteUserEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete(
        "api/admin/users",
        async (string email, ISender sender) =>
        {
          var command = new AdminDeleteUser.Command { Email = email };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminDeleteUser");
  }
}

public class AdminDeleteUserResponse { }
