using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;
using AdminDeleteSeasonResult = Microsoft.AspNetCore.Http.HttpResults.Results<
  Microsoft.AspNetCore.Http.HttpResults.Ok<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminDeleteSeasonResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.NotFound<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminDeleteSeasonResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.BadRequest<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminDeleteSeasonResponse>>
>;

namespace RSPWebAPI.Features.Seasons;

public static class AdminDeleteSeason
{
  public class Command : AdminAuthRequest<ApiResult<AdminDeleteSeasonResponse>>
  {
    public string SeasonId { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.SeasonId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminDeleteSeasonResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminDeleteSeasonResponse>> Handle(
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
        return new ApiResult<AdminDeleteSeasonResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.SeasonDoesNotExists),
        };
      }

      try
      {
        _dbContext.Remove(existingSeason);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminDeleteSeasonResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.SeasonDeletedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.SeasonDeletionUnexpectedError);

        return new ApiResult<AdminDeleteSeasonResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.SeasonDeletionUnexpectedError),
        };
      }
    }
  }
}

public class AdminDeleteSeasonEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete(
        "api/admin/seasons",
        async Task<AdminDeleteSeasonResult> (string seasonId, ISender sender) =>
        {
          var command = new AdminDeleteSeason.Command { SeasonId = seasonId };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminDeleteSeason");
  }
}

public class AdminDeleteSeasonResponse { }
