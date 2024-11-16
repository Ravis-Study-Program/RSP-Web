using System.ComponentModel.DataAnnotations;
using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Users;

public static class GetCurrentUser
{
  public class Command : AuthRequest<ApiResult<GetCurrentUserResponse>>
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

  public class Handler : IRequestHandler<Command, ApiResult<GetCurrentUserResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetCurrentUserResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(
          u => u.Email == request.Email,
          cancellationToken
        );
        if (existingUser == null)
        {
          return new ApiResult<GetCurrentUserResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.UserEmailDoesNotExists),
          };
        }

        return new ApiResult<GetCurrentUserResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new GetCurrentUserResponse { User = existingUser },
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.UserListUnexpectedError);

        return new ApiResult<GetCurrentUserResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.UserListUnexpectedError),
        };
      }
    }
  }
}

public class GetCurrentUserEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
        "api/users/get-current-user",
        async (string? email, ISender sender, HttpContext httpContext) =>
        {
          var currentUserEmail = httpContext?.User?.Identity?.Name ?? "";

          var command = new GetCurrentUser.Command { Email = email ?? currentUserEmail };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("GetCurrentUser");
  }
}

public record GetCurrentUserResponse
{
  [Required]
  public UserEntity User { get; set; }
}
