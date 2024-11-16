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

namespace RSPWebAPI.Features.SeasonUsers;

public static class GetSeasonUsers
{
  public class Command : AuthRequest<ApiResult<GetSeasonUsersResponse>>
  {
    public string SeasonSlug { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.SeasonSlug).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<GetSeasonUsersResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetSeasonUsersResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var seasonUsers = await _dbContext
                              .Enrollments
                              .Where(e => e.Season.Slug == request.SeasonSlug)
                              .Select(e => new SeasonUserDto
                              {
                                Name = e.User.Name,
                                Role = e.Role,
                                DiscordId = e.User.DiscordId,
                                ProfileImage = e.User.ProfileImage,
                                Email = e.User.Email
                              })
                              .AsNoTracking()
                              .ToListAsync(cancellationToken);

        return new ApiResult<GetSeasonUsersResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new GetSeasonUsersResponse
          {
            SeasonUsers = seasonUsers
          },
          SuccessMessage = Message.SeasonUsersListSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.SeasonUsersListUnexpectedError);

        return new ApiResult<GetSeasonUsersResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.SeasonUsersListUnexpectedError)
        };
      }
    }
  }
}

public class GetSeasonUsersEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/season-users/{seasonSlug}",
         async (string seasonSlug, ISender sender, HttpContext httpContext) =>
         {
           var command = new GetSeasonUsers.Command
           {
             SeasonSlug = seasonSlug
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("GetSeasonUsers");
  }
}

public record GetSeasonUsersRequest
{
}

public record SeasonUserDto
{
  [Required] public string DiscordId { get; set; } = string.Empty;
  [Required] public string Name { get; set; } = string.Empty;
  [Required] public string Email { get; set; } = string.Empty;
  [Required] public SeasonRole Role { get; set; }
  [Required] public string ProfileImage { get; set; } = string.Empty;
}

public class GetSeasonUsersResponse
{
  [Required] public IList<SeasonUserDto> SeasonUsers { get; set; } = new List<SeasonUserDto>();
}
