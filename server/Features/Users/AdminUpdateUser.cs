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

public static class AdminUpdateUser
{
  public class Command : AdminAuthRequest<ApiResult<AdminUpdateUserResponse>>
  {
    public string DiscordId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminUpdateUserResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminUpdateUserResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingUser = await _dbContext
                               .Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
      if (existingUser == null)
      {
        return new ApiResult<AdminUpdateUserResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.UserEmailDoesNotExists)
        };
      }

      existingUser.DiscordId = request.DiscordId;
      existingUser.Name = request.Name;
      existingUser.ProfileImage = request.ProfileImage;
      existingUser.IsAdmin = request.IsAdmin;

      try
      {
        _dbContext.Update(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminUpdateUserResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminUpdateUserResponse
          {
            UserId = existingUser.UserId,
            DiscordId = existingUser.DiscordId,
            Email = existingUser.Email,
            Name = existingUser.Name,
            ProfileImage = existingUser.ProfileImage,
            IsAdmin = existingUser.IsAdmin
          },
          SuccessMessage = Message.UserUpdatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.UserUpdateUnexpectedError);

        return new ApiResult<AdminUpdateUserResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.UserUpdateUnexpectedError)
        };
      }
    }
  }
}

public class AdminUpdateUserEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPut(
         "api/admin/users",
         async (AdminUpdateUserRequest request, ISender sender) =>
         {
           var command = new AdminUpdateUser.Command
           {
             DiscordId = request.DiscordId,
             Email = request.Email,
             Name = request.Name,
             ProfileImage = request.ProfileImage,
             IsAdmin = request.IsAdmin
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminUpdateUser");
  }
}

public record AdminUpdateUserRequest
{
  public string DiscordId { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string ProfileImage { get; set; } = string.Empty;
  public bool IsAdmin { get; set; } = false;
}

public class AdminUpdateUserResponse
{
  public Guid UserId { get; set; }
  public string DiscordId { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string ProfileImage { get; set; } = string.Empty;
  public bool IsAdmin { get; set; }
}