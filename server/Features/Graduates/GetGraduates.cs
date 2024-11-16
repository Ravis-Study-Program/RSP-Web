using System.ComponentModel.DataAnnotations;
using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Graduates;

public static class GetGraduates
{
  public class Command : AuthRequest<ApiResult<GetGraduatesResponse>>
  {
  }

  public class Validator : AbstractValidator<Command>
  {
  }

  public class Handler : IRequestHandler<Command, ApiResult<GetGraduatesResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetGraduatesResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var graduates = await _dbContext
                              .Users
                              .Select(u => new GraduateDto
                              {
                                Name = u.Name,
                                DiscordId = u.DiscordId,
                                ProfileImage = u.ProfileImage,
                                Email = u.Email
                              })
                              .AsNoTracking()
                              .ToListAsync(cancellationToken);

        return new ApiResult<GetGraduatesResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new GetGraduatesResponse
          {
            Graduates = graduates
          },
          SuccessMessage = Message.GraduatesListSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.GraduatesListUnexpectedError);

        return new ApiResult<GetGraduatesResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.GraduatesListUnexpectedError)
        };
      }
    }
  }
}

public class GetGraduatesEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/graduates",
         async (ISender sender, HttpContext httpContext) =>
         {
           var command = new GetGraduates.Command();
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("GetGraduates");
  }
}

public record GetGraduatesRequest
{
}

public record GraduateDto
{
  [Required] public string DiscordId { get; set; } = string.Empty;
  [Required] public string Name { get; set; } = string.Empty;
  [Required] public string Email { get; set; } = string.Empty;
  [Required] public string ProfileImage { get; set; } = string.Empty;
}

public class GetGraduatesResponse
{
  [Required] public IList<GraduateDto> Graduates { get; set; } = new List<GraduateDto>();
}
