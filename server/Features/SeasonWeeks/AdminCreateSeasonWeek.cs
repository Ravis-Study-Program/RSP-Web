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
using AdminCreateSeasonWeekResult = Microsoft.AspNetCore.Http.HttpResults.Results<
  Microsoft.AspNetCore.Http.HttpResults.Ok<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminCreateSeasonWeekResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.NotFound<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminCreateSeasonWeekResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.BadRequest<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminCreateSeasonWeekResponse>>
>;

namespace RSPWebAPI.Features.SeasonWeeks;

public static class AdminCreateSeasonWeek
{
  public class Command : AdminAuthRequest<ApiResult<AdminCreateSeasonWeekResponse>>
  {
    public string SeasonId { get; set; } = string.Empty;
    public int WeekNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.SeasonId).NotEmpty();
      RuleFor(c => c.WeekNumber).GreaterThan(0);
      RuleFor(c => c.StartDate).NotEmpty();
      RuleFor(c => c.EndDate)
        .NotEmpty()
        .GreaterThan(c => c.StartDate)
        .WithMessage(Message.EndDateMustBeGreaterThanStartDate);
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminCreateSeasonWeekResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminCreateSeasonWeekResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var season = await _dbContext.Seasons.FirstOrDefaultAsync(
        s => s.SeasonId == request.SeasonId,
        cancellationToken
      );
      if (season == null)
      {
        return new ApiResult<AdminCreateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.SeasonDoesNotExists),
        };
      }

      var existingWeek = await _dbContext.SeasonWeeks.FirstOrDefaultAsync(
        w => w.SeasonId == request.SeasonId && w.WeekNumber == request.WeekNumber,
        cancellationToken
      );
      if (existingWeek != null)
      {
        return new ApiResult<AdminCreateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.SeasonWeekWeekNumberAlreadyExists),
        };
      }

      if (
        request.StartDate < season.StartDateInclusiveUtc
        || request.EndDate > season.EndDateInclusiveUtc
      )
      {
        return new ApiResult<AdminCreateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.SeasonWeekDatesNotWithinSeasonDates),
        };
      }

      var seasonWeek = new SeasonWeekEntity
      {
        SeasonWeekId = Database.Constants.GeneratePrimaryKeyId(),
        SeasonId = request.SeasonId,
        WeekNumber = request.WeekNumber,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
      };

      try
      {
        _dbContext.Add(seasonWeek);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminCreateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminCreateSeasonWeekResponse
          {
            SeasonWeekId = seasonWeek.SeasonWeekId,
            SeasonId = seasonWeek.SeasonId,
            WeekNumber = seasonWeek.WeekNumber,
            StartDate = seasonWeek.StartDate,
            EndDate = seasonWeek.EndDate,
          },
          SuccessMessage = Message.SeasonWeekCreatedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.SeasonWeekCreationUnexpectedError);

        return new ApiResult<AdminCreateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.SeasonWeekCreationUnexpectedError),
        };
      }
    }
  }
}

public class AdminCreateSeasonWeekEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
        "api/admin/season-weeks",
        async Task<AdminCreateSeasonWeekResult> (
          AdminCreateSeasonWeekRequest request,
          ISender sender
        ) =>
        {
          var command = new AdminCreateSeasonWeek.Command
          {
            SeasonId = request.SeasonId,
            WeekNumber = request.WeekNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminCreateSeasonWeek");
  }
}

public record AdminCreateSeasonWeekRequest
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public int WeekNumber { get; set; }

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public DateTime EndDate { get; set; }
}

public class AdminCreateSeasonWeekResponse
{
  [Required]
  public string SeasonWeekId { get; set; } = string.Empty;

  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public int WeekNumber { get; set; }

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public DateTime EndDate { get; set; }
}
