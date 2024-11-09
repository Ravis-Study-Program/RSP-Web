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

public static class AdminCreateUser
{
  public class Command : AdminAuthRequest<ApiResult<AdminCreateUserResponse>>
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
      RuleFor(c => c.DiscordId).NotEmpty();
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminCreateUserResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminCreateUserResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingUser = await _dbContext
                               .Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

      if (existingUser != null)
      {
        return new ApiResult<AdminCreateUserResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.UserEmailExists)
        };
      }

      var user = new UserEntity
      {
        UserId = Database.Constants.GeneratePrimaryKeyId(),
        DiscordId = request.DiscordId,
        Email = request.Email,
        Name = request.Name,
        ProfileImage = request.ProfileImage,
        IsAdmin = request.IsAdmin
      };

      try
      {
        _dbContext.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminCreateUserResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminCreateUserResponse
          {
            UserId = user.UserId,
            DiscordId = user.DiscordId,
            Email = user.Email,
            Name = user.Name,
            ProfileImage = user.ProfileImage,
            IsAdmin = user.IsAdmin
          },
          SuccessMessage = Message.UserCreatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.UserCreationUnexpectedError);

        return new ApiResult<AdminCreateUserResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.UserCreationUnexpectedError)
        };
      }
    }
  }
}

public class AdminCreateUserEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
         "api/admin/users",
         async (AdminCreateUserRequest request, ISender sender) =>
         {
           var command = new AdminCreateUser.Command
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
       .WithName("AdminCreateUser");
  }
}

public record AdminCreateUserRequest
{
  [Required] public string DiscordId { get; set; } = string.Empty;
  [Required] public string Email { get; set; } = string.Empty;
  [Required] public string Name { get; set; } = string.Empty;
  [Required] public string ProfileImage { get; set; } = string.Empty;
  [Required] public bool IsAdmin { get; set; } = false;
}

public class AdminCreateUserResponse
{
  [Required] public string UserId { get; set; } = string.Empty;
  [Required] public string DiscordId { get; set; } = string.Empty;
  [Required] public string Email { get; set; } = string.Empty;
  [Required] public string Name { get; set; } = string.Empty;
  [Required] public string ProfileImage { get; set; } = string.Empty;
  [Required] public bool IsAdmin { get; set; }
}
