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

namespace RSPWebAPI.Features.MockInterviews;

public static class MockCreateMockInterview
{
  public class Command : AuthRequest<ApiResult<MockCreateMockInterviewResponse>>
  {
    public string Email { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<MockCreateMockInterviewResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<MockCreateMockInterviewResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var interviewee = await _dbContext
                                .Users
                                .Where(u => u.Email == request.Email)
                                .Select(u => new UserEntity
                                {
                                  UserId = u.UserId
                                })
                                .FirstOrDefaultAsync(cancellationToken);
        if (interviewee == null)
        {
          return new ApiResult<MockCreateMockInterviewResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.UserEmailDoesNotExists)
          };
        }

        // Process each mock interview round
        var mockInterviewRounds = new List<MockInterviewRoundEntity>();
        for (var j = 0; j < 6; j++)
        {
          var mockInterviewRound = new MockInterviewRoundEntity
          {
            IsReviewedByInterviewee = false,
            IntervieweeComment = "My own comment"
          };

          if (j == 0)
          {
            mockInterviewRound.BehaviouralMockInterviewRound = new BehaviouralMockInterviewRoundEntity
            {
              BehavioralScore = 8
            };
          }
          else if (j % 2 == 0)
          {
            // get random leetcode
            var leetcode = await _dbContext.LeetcodeProblems.Take(1).ToListAsync(cancellationToken);
            if (leetcode.Count == 0)
            {
              return new ApiResult<MockCreateMockInterviewResponse>
              {
                StatusCode = HttpStatusCode.BadRequest,
                Error = new ApiError("Something went wrong")
              };
            }

            mockInterviewRound.LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundEntity
            {
              AlgorithmDesignScore = 8,
              ConfirmQuestionScore = 10,
              ComplexityAnalysisScore = 5,
              CodingScore = 3,
              TestingScore = 1,
              LeetcodeProblemId = leetcode[0].LeetcodeProblemId
            };
          }
          else if (j == 1)
          {
            mockInterviewRound.CustomMockInterviewRound = new CustomMockInterviewRoundEntity
            {
              Content = "Some random content",
              Link = "google.com"
            };
          }

          mockInterviewRounds.Add(mockInterviewRound);
        }

        var mockInterview = new MockInterviewEntity
        {
          IsPass = true,
          InterviewerUserId = interviewee.UserId,
          IntervieweeUserId = interviewee.UserId,
          MockInterviewRounds = mockInterviewRounds,
          StartDate = DateTime.UtcNow,
          TimeTakenInMinutes = 120
        };

        _dbContext.Add(mockInterview);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<MockCreateMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.MockInterviewCreatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MockInterviewCreationUnexpectedError);

        return new ApiResult<MockCreateMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MockInterviewCreationUnexpectedError)
        };
      }
    }
  }
}

public class MockCreateMockInterviewEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
         "api/mock-interview/mock-create",
         async (ISender sender, HttpContext httpContext) =>
         {
           var email = httpContext?.User?.Identity?.Name ?? "";

           var command = new MockCreateMockInterview.Command
           {
             Email = email
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("MockCreateMockInterview");
  }
}

public class MockCreateMockInterviewResponse
{
}
