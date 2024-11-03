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

public static class CreateUserIfNotExists
{
  public class Command : AuthRequest<ApiResult<CreateUserIfNotExistsResponse>>
  {
    public string DiscordId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<CreateUserIfNotExistsResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<CreateUserIfNotExistsResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingUser = await _dbContext
                               .Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
      if (existingUser == null)
      {
        var user = new UserEntity
        {
          UserId = Database.Constants.GeneratePrimaryKeyId(),
          DiscordId = request.DiscordId,
          Email = request.Email,
          Name = request.Name,
          ProfileImage = request.ProfileImage
        };

        try
        {
          _dbContext.Add(user);
          await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, Message.UserCreationUnexpectedError);

          return new ApiResult<CreateUserIfNotExistsResponse>
          {
            StatusCode = HttpStatusCode.InternalServerError,
            Error = new ApiError(Message.UserCreationUnexpectedError)
          };
        }
      }

      return new ApiResult<CreateUserIfNotExistsResponse>
      {
        StatusCode = HttpStatusCode.OK,
        ResponseBody = null
      };
    }
  }
}

public class CreateUserIfNotExistsEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
         "api/users/create-if-not-exists",
         async (CreateUserIfNotExistsRequest request, ISender sender, HttpContext httpContext) =>
         {
           var email = httpContext?.User?.Identity?.Name ?? "";

           var command = new CreateUserIfNotExists.Command
           {
             DiscordId = request.DiscordId,
             Email = email,
             Name = request.Name,
             ProfileImage = request.ProfileImage
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("CreateUserIfNotExists");
  }
}

public record CreateUserIfNotExistsRequest
{
  public string DiscordId { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string ProfileImage { get; set; } = string.Empty;
}

public record CreateUserIfNotExistsResponse
{
}
