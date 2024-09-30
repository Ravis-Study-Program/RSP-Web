using System.Net;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;
using AdminListSeasonResult = Microsoft.AspNetCore.Http.HttpResults.Results<
  Microsoft.AspNetCore.Http.HttpResults.Ok<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminListSeasonResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.NotFound<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminListSeasonResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.BadRequest<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminListSeasonResponse>>
>;

namespace RSPWebAPI.Features.Seasons;

public static class AdminListSeason
{
  public class Command : AdminAuthRequest<ApiResult<AdminListSeasonResponse>>
  {
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminListSeasonResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminListSeasonResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var seasons = await _dbContext.Seasons.ToListAsync(cancellationToken);

        return new ApiResult<AdminListSeasonResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.SeasonListSuccessfully,
          ResponseBody = new AdminListSeasonResponse
          {
            Seasons = seasons
          }
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.SeasonListUnexpectedError);

        return new ApiResult<AdminListSeasonResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.SeasonListUnexpectedError)
        };
      }
    }
  }
}

public class AdminListSeasonEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/admin/seasons",
         async Task<AdminListSeasonResult> (ISender sender) =>
         {
           var command = new AdminListSeason.Command();
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminListSeason");
  }
}

public class AdminListSeasonResponse
{
  public ICollection<Season> Seasons { get; set; } = new List<Season>();
}
