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

namespace RSPWebAPI.Features.Leetcode;

public static class GetLeetcodeProblems
{
  public class Command : AuthRequest<ApiResult<GetLeetcodeProblemsResponse>> { }

  public class Validator : AbstractValidator<Command> { }

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
          .LeetcodeProblems.Select(l => new LeetcodeProblemDto
          {
            LeetcodeProblemId = l.LeetcodeProblemId,
            IsPremium = l.IsPremium,
            Link = l.Problem.Link ?? "",
            Title = l.Problem.Title,
            Difficulty = l.LeetcodeProblemDifficulty,
            LeetcodeProblemCategories = l
              .LeetcodeProblemCategories.Select(c => new LeetcodeProblemCategoryDto
              {
                Name = c.Name,
              })
              .ToList(),
          })
          .AsNoTracking()
          .ToListAsync(cancellationToken);

        return new ApiResult<GetLeetcodeProblemsResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new GetLeetcodeProblemsResponse { LeetcodeProblems = leetcodeProblems },
          SuccessMessage = Message.LeetcodeProblemsListSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.LeetcodeProblemsListUnexpectedError);

        return new ApiResult<GetLeetcodeProblemsResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.LeetcodeProblemsListUnexpectedError),
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

public record GetLeetcodeProblemsRequest { }

public record LeetcodeProblemDto
{
  [Required]
  public List<LeetcodeProblemCategoryDto> LeetcodeProblemCategories = new();

  [Required]
  public string LeetcodeProblemId { get; set; } = string.Empty;

  [Required]
  public LeetcodeProblemDifficulty Difficulty { get; set; }

  [Required]
  public bool IsPremium { get; set; }

  [Required]
  public string Title { get; set; } = string.Empty;

  [Required]
  public string Link { get; set; } = string.Empty;
}

public record LeetcodeProblemCategoryDto
{
  [Required]
  public string Name { get; set; } = string.Empty;
}

public class GetLeetcodeProblemsResponse
{
  [Required]
  public IList<LeetcodeProblemDto> LeetcodeProblems { get; set; } = new List<LeetcodeProblemDto>();
}
