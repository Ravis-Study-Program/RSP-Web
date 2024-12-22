using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;
using AdminDeleteSeasonWeekResult = Microsoft.AspNetCore.Http.HttpResults.Results<
  Microsoft.AspNetCore.Http.HttpResults.Ok<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminDeleteSeasonWeekResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.NotFound<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminDeleteSeasonWeekResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.BadRequest<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.SeasonWeeks.AdminDeleteSeasonWeekResponse>>
>;

namespace RSPWebAPI.Features.SeasonWeeks;

public static class AdminDeleteSeasonWeek
{
  public class Command : AdminAuthRequest<ApiResult<AdminDeleteSeasonWeekResponse>>
  {
    public string SeasonWeekId { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.SeasonWeekId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminDeleteSeasonWeekResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminDeleteSeasonWeekResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingWeek = await _dbContext.SeasonWeeks.FirstOrDefaultAsync(
        sw => sw.SeasonWeekId == request.SeasonWeekId,
        cancellationToken
      );

      if (existingWeek == null)
      {
        return new ApiResult<AdminDeleteSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.SeasonWeekDoesNotExists),
        };
      }

      try
      {
        _dbContext.Remove(existingWeek);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminDeleteSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.SeasonWeekDeletedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.SeasonWeekDeletedSuccessfully);

        return new ApiResult<AdminDeleteSeasonWeekResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.SeasonWeekDeletedSuccessfully),
        };
      }
    }
  }
}

public class AdminDeleteSeasonWeekEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete(
        "api/admin/season-weeks",
        async Task<AdminDeleteSeasonWeekResult> (string seasonWeekId, ISender sender) =>
        {
          var command = new AdminDeleteSeasonWeek.Command { SeasonWeekId = seasonWeekId };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminDeleteSeasonWeek");
  }
}

public class AdminDeleteSeasonWeekResponse { }
