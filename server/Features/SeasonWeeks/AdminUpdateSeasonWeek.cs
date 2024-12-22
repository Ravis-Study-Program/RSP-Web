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
using AdminUpdateSeasonWeekResult = Microsoft.AspNetCore.Http.HttpResults.Results<
  Microsoft.AspNetCore.Http.HttpResults.Ok<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminUpdateSeasonWeekResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.NotFound<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminUpdateSeasonWeekResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.BadRequest<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminUpdateSeasonWeekResponse>>
>;

namespace RSPWebAPI.Features.SeasonWeeks;

public static class AdminUpdateSeasonWeek
{
  public class Command : AdminAuthRequest<ApiResult<AdminUpdateSeasonWeekResponse>>
  {
    public string SeasonWeekId { get; set; } = string.Empty;
    public int WeekNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.SeasonWeekId).NotEmpty();
      RuleFor(c => c.WeekNumber).GreaterThan(0);
      RuleFor(c => c.StartDate).NotEmpty();
      RuleFor(c => c.EndDate)
        .NotEmpty()
        .GreaterThan(c => c.StartDate)
        .WithMessage(Message.EndDateMustBeGreaterThanStartDate);
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminUpdateSeasonWeekResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminUpdateSeasonWeekResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingWeek = await _dbContext
        .SeasonWeeks.Include(sw => sw.Season)
        .FirstOrDefaultAsync(sw => sw.SeasonWeekId == request.SeasonWeekId, cancellationToken);

      if (existingWeek == null)
      {
        return new ApiResult<AdminUpdateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.SeasonWeekDoesNotExists),
        };
      }

      var conflictingWeek = await _dbContext.SeasonWeeks.FirstOrDefaultAsync(
        w =>
          w.SeasonId == existingWeek.SeasonId
          && w.WeekNumber == request.WeekNumber
          && w.SeasonWeekId != request.SeasonWeekId,
        cancellationToken
      );
      if (conflictingWeek != null)
      {
        return new ApiResult<AdminUpdateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.SeasonWeekWeekNumberAlreadyExists),
        };
      }

      if (
        request.StartDate < existingWeek.Season.StartDateInclusiveUtc
        || request.EndDate > existingWeek.Season.EndDateInclusiveUtc
      )
      {
        return new ApiResult<AdminUpdateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.SeasonWeekDatesNotWithinSeasonDates),
        };
      }

      try
      {
        existingWeek.WeekNumber = request.WeekNumber;
        existingWeek.StartDate = request.StartDate;
        existingWeek.EndDate = request.EndDate;

        _dbContext.Update(existingWeek);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminUpdateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminUpdateSeasonWeekResponse
          {
            SeasonWeekId = existingWeek.SeasonWeekId,
            SeasonId = existingWeek.SeasonId,
            WeekNumber = existingWeek.WeekNumber,
            StartDate = existingWeek.StartDate,
            EndDate = existingWeek.EndDate,
          },
          SuccessMessage = Message.SeasonWeekUpdateSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.SeasonWeekUpdateUnexpectedError);

        return new ApiResult<AdminUpdateSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.SeasonWeekUpdateUnexpectedError),
        };
      }
    }
  }
}

public class AdminUpdateSeasonWeekEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPut(
        "api/admin/season-weeks",
        async Task<AdminUpdateSeasonWeekResult> (
          AdminUpdateSeasonWeekRequest request,
          ISender sender
        ) =>
        {
          var command = new AdminUpdateSeasonWeek.Command
          {
            SeasonWeekId = request.SeasonWeekId,
            WeekNumber = request.WeekNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminUpdateSeasonWeek");
  }
}

public record AdminUpdateSeasonWeekRequest
{
  [Required]
  public string SeasonWeekId { get; set; } = string.Empty;

  [Required]
  public int WeekNumber { get; set; }

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public DateTime EndDate { get; set; }
}

public class AdminUpdateSeasonWeekResponse
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
