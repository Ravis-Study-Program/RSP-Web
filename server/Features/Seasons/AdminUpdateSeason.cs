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
using AdminUpdateSeasonResult = Microsoft.AspNetCore.Http.HttpResults.Results<
  Microsoft.AspNetCore.Http.HttpResults.Ok<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminUpdateSeasonResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.NotFound<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminUpdateSeasonResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.BadRequest<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminUpdateSeasonResponse>>
>;

namespace RSPWebAPI.Features.Seasons;

public static class AdminUpdateSeason
{
  public class Command : AdminAuthRequest<ApiResult<AdminUpdateSeasonResponse>>
  {
    public string SeasonId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime StartDateInclusiveUtc { get; set; }
    public DateTime EndDateInclusiveUtc { get; set; }
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.SeasonId).NotEmpty();
      RuleFor(c => c.Name).NotEmpty();
      RuleFor(c => c.Slug).NotEmpty();
      RuleFor(c => c.StartDateInclusiveUtc).NotEmpty();
      RuleFor(c => c.EndDateInclusiveUtc).NotEmpty();
      RuleFor(c => c.Location).NotEmpty();
      RuleFor(c => c.ImageUrl).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminUpdateSeasonResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminUpdateSeasonResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingSeason = await _dbContext.Seasons.FirstOrDefaultAsync(
        u => u.SeasonId == request.SeasonId,
        cancellationToken
      );
      if (existingSeason == null)
      {
        return new ApiResult<AdminUpdateSeasonResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.SeasonDoesNotExists),
        };
      }

      existingSeason.Name = request.Name;
      existingSeason.Slug = request.Slug;
      existingSeason.StartDateInclusiveUtc = request.StartDateInclusiveUtc;
      existingSeason.EndDateInclusiveUtc = request.EndDateInclusiveUtc;
      existingSeason.Location = request.Location;
      existingSeason.ImageUrl = request.ImageUrl;

      try
      {
        _dbContext.Update(existingSeason);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminUpdateSeasonResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminUpdateSeasonResponse
          {
            SeasonId = existingSeason.SeasonId,
            Name = existingSeason.Name,
            Slug = existingSeason.Slug,
            StartDateInclusiveUtc = existingSeason.StartDateInclusiveUtc,
            EndDateInclusiveUtc = existingSeason.EndDateInclusiveUtc,
            Location = existingSeason.Location,
            ImageUrl = existingSeason.ImageUrl,
          },
          SuccessMessage = Message.SeasonUpdatedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.SeasonCreationUnexpectedError);

        return new ApiResult<AdminUpdateSeasonResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.SeasonCreationUnexpectedError),
        };
      }
    }
  }
}

public class AdminUpdateSeasonEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPut(
        "api/admin/seasons",
        async Task<AdminUpdateSeasonResult> (AdminUpdateSeasonRequest request, ISender sender) =>
        {
          var command = new AdminUpdateSeason.Command
          {
            SeasonId = request.SeasonId,
            Name = request.Name,
            Slug = request.Slug,
            StartDateInclusiveUtc = request.StartDateInclusiveUtc,
            EndDateInclusiveUtc = request.EndDateInclusiveUtc,
            Location = request.Location,
            ImageUrl = request.ImageUrl,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminUpdateSeason");
  }
}

public record AdminUpdateSeasonRequest
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Slug { get; set; } = string.Empty;

  [Required]
  public DateTime StartDateInclusiveUtc { get; set; }

  [Required]
  public DateTime EndDateInclusiveUtc { get; set; }

  [Required]
  public string Location { get; set; } = string.Empty;

  [Required]
  public string ImageUrl { get; set; } = string.Empty;
}

public class AdminUpdateSeasonResponse
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Slug { get; set; } = string.Empty;

  [Required]
  public DateTime StartDateInclusiveUtc { get; set; }

  [Required]
  public DateTime EndDateInclusiveUtc { get; set; }

  [Required]
  public string Location { get; set; } = string.Empty;

  [Required]
  public string ImageUrl { get; set; } = string.Empty;
}
