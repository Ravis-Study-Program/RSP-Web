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

public static class GenerateLeetcodeProblemRecommendations
{
  public class Command : AuthRequest<ApiResult<GenerateLeetcodeProblemRecommendationsResponse>>
  {
    public string UserId { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.UserId).NotEmpty();
    }
  }

  public class Handler
    : IRequestHandler<Command, ApiResult<GenerateLeetcodeProblemRecommendationsResponse>>
  {
    private readonly ApplicationDbContext _dbContext;

    public Handler(ApplicationDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task<ApiResult<GenerateLeetcodeProblemRecommendationsResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var existingRecommendation =
          await _dbContext.LeetcodeProblemRecommendations.FirstOrDefaultAsync(
            r => r.UserId == request.UserId && r.ProblemAttemptId == null,
            cancellationToken
          );
        if (existingRecommendation != null)
        {
          // Exit early if user hasn't completed attempt of existing recommendation
          var attempt = await _dbContext.ProblemAttempts.FirstOrDefaultAsync(
            p =>
              p.LeetcodeProblemId == existingRecommendation.LeetcodeProblemId
              && p.UserId == request.UserId,
            cancellationToken
          );
          if (attempt == null)
          {
            return new ApiResult<GenerateLeetcodeProblemRecommendationsResponse>
            {
              StatusCode = HttpStatusCode.OK,
              SuccessMessage = Message.LeetcodeProblemRecommenderCreatedSuccessfully,
            };
          }
        }

        // Generate recommendation randomly
        var problemRecommendationId = await _dbContext
          .LeetcodeProblems.Where(lp =>
            !_dbContext.ProblemAttempts.Any(pa =>
              pa.UserId == request.UserId && pa.LeetcodeProblemId == lp.LeetcodeProblemId
            )
          )
          .Select(lp => lp.LeetcodeProblemId)
          .FirstOrDefaultAsync(cancellationToken);
        if (problemRecommendationId == null)
        {
          return new ApiResult<GenerateLeetcodeProblemRecommendationsResponse>
          {
            StatusCode = HttpStatusCode.OK,
            SuccessMessage = Message.LeetcodeProblemRecommenderNoLeetcodeProblemLeft,
          };
        }

        var recommendation = new LeetcodeProblemRecommendationEntity
        {
          LeetcodeProblemRecommendationId = Database.Constants.GeneratePrimaryKeyId(),
          UserId = request.UserId,
          LeetcodeProblemId = problemRecommendationId,
        };
        _dbContext.LeetcodeProblemRecommendations.Add(recommendation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<GenerateLeetcodeProblemRecommendationsResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.LeetcodeProblemRecommenderCreatedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        return new ApiResult<GenerateLeetcodeProblemRecommendationsResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.LeetcodeProblemRecommenderUnexpectedError),
        };
      }
    }
  }
}

public class GenerateLeetcodeProblemRecommendationsResponse { }
