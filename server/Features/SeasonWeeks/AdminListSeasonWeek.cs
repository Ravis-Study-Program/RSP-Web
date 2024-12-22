using System.ComponentModel.DataAnnotations;
using System.Net;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;
using AdminListSeasonWeekResult = Microsoft.AspNetCore.Http.HttpResults.Results<
  Microsoft.AspNetCore.Http.HttpResults.Ok<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminListSeasonWeekResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.NotFound<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminListSeasonWeekResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.BadRequest<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminListSeasonWeekResponse>>
>;

namespace RSPWebAPI.Features.SeasonWeeks;

public static class AdminListSeasonWeek
{
  public class Command : AdminAuthRequest<ApiResult<AdminListSeasonWeekResponse>>
  {
    public string? SeasonId { get; set; }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminListSeasonWeekResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminListSeasonWeekResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var seasonWeeks = await _dbContext.SeasonWeeks.ToListAsync(cancellationToken);

        return new ApiResult<AdminListSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.SeasonWeekListSuccessfully,
          ResponseBody = new AdminListSeasonWeekResponse { SeasonWeeks = seasonWeeks },
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.SeasonWeekListUnexpectedError);

        return new ApiResult<AdminListSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.SeasonWeekListUnexpectedError),
        };
      }
    }
  }
}

public class AdminListSeasonWeekEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
        "api/admin/season-weeks",
        async Task<AdminListSeasonWeekResult> (string? seasonId, ISender sender) =>
        {
          var command = new AdminListSeasonWeek.Command { SeasonId = seasonId };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminListSeasonWeek");
  }
}

public class AdminListSeasonWeekResponse
{
  [Required]
  public ICollection<SeasonWeekEntity> SeasonWeeks { get; set; } = new List<SeasonWeekEntity>();
}
