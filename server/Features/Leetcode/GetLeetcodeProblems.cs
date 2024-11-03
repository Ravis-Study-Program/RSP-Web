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

namespace RSPWebAPI.Features.Leetcode;

public static class GetLeetcodeProblems
{
  public class Command : AuthRequest<ApiResult<GetLeetcodeProblemsResponse>>
  {
  }

  public class Validator : AbstractValidator<Command>
  {
  }

  public class Handler : IRequestHandler<Command, ApiResult<GetLeetcodeProblemsResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetLeetcodeProblemsResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var leetcodeProblems = await _dbContext
                                     .LeetcodeProblems
                                     .Include(l => l.LeetcodeProblemCategories)
                                     .Include(l => l.LeetcodeProblemDifficulty)
                                     .Include(l => l.Problem)
                                     .ToListAsync(cancellationToken);

        return new ApiResult<GetLeetcodeProblemsResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new GetLeetcodeProblemsResponse
          {
            LeetcodeProblems = leetcodeProblems
          },
          SuccessMessage = Message.LeetcodeProblemsListSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.LeetcodeProblemsListUnexpectedError);

        return new ApiResult<GetLeetcodeProblemsResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.LeetcodeProblemsListUnexpectedError)
        };
      }
    }
  }
}

public class GetLeetcodeProblemsEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/leetcode-problems",
         async (ISender sender, HttpContext httpContext) =>
         {
           var command = new GetLeetcodeProblems.Command();
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("GetLeetcodeProblems");
  }
}

public record GetLeetcodeProblemsRequest
{
}

public class GetLeetcodeProblemsResponse
{
  public IList<LeetcodeProblemEntity> LeetcodeProblems { get; set; } = new List<LeetcodeProblemEntity>();
}
