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

namespace RSPWebAPI.Features.MockInterviews;

public static class CreateMockInterview
{
  public class Command : AuthRequest<ApiResult<CreateMockInterviewResponse>>
  {
    public string InterviewerUserId { get; set; } = string.Empty;
    public string IntervieweeEmail { get; set; } = string.Empty;
    public string? EnrollmentId { get; set; }
    public DateTime StartDate { get; set; }
    public int TimeTakenInMinutes { get; set; }
    public List<MockInterviewRoundDto> MockInterviewRoundDtos { get; set; } = new();
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.InterviewerUserId).NotEmpty();
      RuleFor(c => c.IntervieweeEmail).NotEmpty().EmailAddress();
      RuleFor(c => c.StartDate).NotEmpty();
      RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThan(0);
      RuleFor(c => c.MockInterviewRoundDtos).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<CreateMockInterviewResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<CreateMockInterviewResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        // Check if the enrollment exists and get season week
        string? seasonWeekId = null;
        if (request.EnrollmentId != null)
        {
          var existingEnrollment = await _dbContext
            .Enrollments.Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EnrollmentId == request.EnrollmentId, cancellationToken);
          if (
            existingEnrollment == null
            || existingEnrollment.User.Email != request.IntervieweeEmail
          )
          {
            return new ApiResult<CreateMockInterviewResponse>
            {
              StatusCode = HttpStatusCode.BadRequest,
              Error = new ApiError(Message.EnrollmentDoesNotExists),
            };
          }

          // Get season week
          var currentDate = DateTime.UtcNow;
          seasonWeekId = await _dbContext
            .SeasonWeeks.Where(s =>
              currentDate >= s.StartDate
              && currentDate <= s.EndDate
              && s.SeasonId == existingEnrollment.SeasonId
            )
            .Select(s => s.SeasonWeekId)
            .FirstOrDefaultAsync(cancellationToken);
          if (seasonWeekId == null)
          {
            return new ApiResult<CreateMockInterviewResponse>
            {
              StatusCode = HttpStatusCode.BadRequest,
              Error = new ApiError(Message.MockInterviewOutOfSeasonDateRange),
            };
          }
        }

        // Get user for interviewee and interviewer
        var intervieweeUserId = await _dbContext
          .Users.Where(u => u.Email == request.IntervieweeEmail)
          .Select(u => u.UserId)
          .FirstOrDefaultAsync(cancellationToken);
        if (intervieweeUserId == null)
        {
          return new ApiResult<CreateMockInterviewResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.UserEmailDoesNotExists),
          };
        }

        var interviewerUserId = await _dbContext
          .Users.Where(u => u.UserId == request.InterviewerUserId)
          .Select(u => u.UserId)
          .FirstOrDefaultAsync(cancellationToken);
        if (interviewerUserId == null)
        {
          return new ApiResult<CreateMockInterviewResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.UserEmailDoesNotExists),
          };
        }

        // Prepare list to store mock interview rounds and related entities
        var mockInterviewRounds = new List<MockInterviewRoundEntity>();
        var mockInterviewId = Database.Constants.GeneratePrimaryKeyId();

        foreach (var m in request.MockInterviewRoundDtos)
        {
          var mockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId();
          var mockInterviewRound = new MockInterviewRoundEntity
          {
            MockInterviewRoundId = mockInterviewRoundId,
            MockInterviewId = mockInterviewId,
            IsReviewedByInterviewee = false,
            IntervieweeComment = "",
          };

          // Add BehaviouralMockInterviewRound if present, and avoid other types in this round
          if (m.BehaviouralMockInterviewRound != null)
          {
            mockInterviewRound.BehaviouralMockInterviewRound =
              new BehaviouralMockInterviewRoundEntity
              {
                BehaviouralMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
                BehavioralScore = m.BehaviouralMockInterviewRound.BehavioralScore,
              };
          }

          // Add LeetcodeMockInterviewRound if present
          if (m.LeetcodeMockInterviewRound != null)
          {
            mockInterviewRound.LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundEntity
            {
              LeetcodeMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
              ConfirmQuestionScore = m.LeetcodeMockInterviewRound.ConfirmQuestionScore,
              AlgorithmDesignScore = m.LeetcodeMockInterviewRound.AlgorithmDesignScore,
              ComplexityAnalysisScore = m.LeetcodeMockInterviewRound.ComplexityAnalysisScore,
              CodingScore = m.LeetcodeMockInterviewRound.CodingScore,
              TestingScore = m.LeetcodeMockInterviewRound.TestingScore,
              LeetcodeProblemId = m.LeetcodeMockInterviewRound.LeetcodeProblemId,
            };
          }

          // Add CustomMockInterviewRound if present
          if (m.CustomMockInterviewRound != null)
          {
            mockInterviewRound.CustomMockInterviewRound = new CustomMockInterviewRoundEntity
            {
              CustomMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
              Content = m.CustomMockInterviewRound.Content,
              Score = m.CustomMockInterviewRound.Score,
              Link = m.CustomMockInterviewRound.Link,
            };
          }

          // Add the constructed mock interview round to the list
          mockInterviewRounds.Add(mockInterviewRound);
        }

        // Create the mock interview entity and add to database
        var mockInterview = new MockInterviewEntity
        {
          MockInterviewId = mockInterviewId,
          IsPass = IsAllScoresAboveThreshold(request),
          InterviewerUserId = interviewerUserId,
          IntervieweeUserId = intervieweeUserId,
          EnrollmentId = request.EnrollmentId,
          MockInterviewRounds = mockInterviewRounds,
          StartDate = request.StartDate,
          TimeTakenInMinutes = request.TimeTakenInMinutes,
          SeasonWeekId = seasonWeekId,
        };

        // Attach mock interview and its rounds to the DbContext to avoid circular dependency issues
        _dbContext.Add(mockInterview);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<CreateMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.MockInterviewCreatedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MockInterviewCreationUnexpectedError);

        return new ApiResult<CreateMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MockInterviewCreationUnexpectedError),
        };
      }
    }

    private static bool IsAllScoresAboveThreshold(Command request)
    {
      const int threshold = 5;

      var numList = new List<int>();
      foreach (var round in request.MockInterviewRoundDtos)
      {
        if (round.LeetcodeMockInterviewRound != null)
        {
          var r = round.LeetcodeMockInterviewRound;
          numList.AddRange(
            new List<int>
            {
              r.ConfirmQuestionScore,
              r.AlgorithmDesignScore,
              r.ComplexityAnalysisScore,
              r.CodingScore,
              r.TestingScore,
            }
          );
        }

        if (round.BehaviouralMockInterviewRound != null)
        {
          var r = round.BehaviouralMockInterviewRound;
          numList.AddRange(new List<int> { r.BehavioralScore });
        }

        if (round.CustomMockInterviewRound != null)
        {
          var r = round.CustomMockInterviewRound;
          numList.AddRange(new List<int> { r.Score });
        }
      }

      foreach (var num in numList)
      {
        if (num < threshold)
        {
          return false;
        }
      }

      return true;
    }
  }
}

public class CreateMockInterviewEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
        "api/mock-interview",
        async (CreateMockInterviewRequest request, ISender sender, HttpContext httpContext) =>
        {
          var email = httpContext?.User?.Identity?.Name ?? "";

          var command = new CreateMockInterview.Command
          {
            InterviewerUserId = request.InterviewerUserId,
            IntervieweeEmail = email,
            EnrollmentId = request.EnrollmentId,
            MockInterviewRoundDtos = request.MockInterviewRoundDtos,
            StartDate = request.StartDate,
            TimeTakenInMinutes = request.TimeTakenInMinutes,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("CreateMockInterview");
  }
}

public record CreateMockInterviewRequest
{
  [Required]
  public string InterviewerUserId { get; set; } = string.Empty;
  public string? EnrollmentId { get; set; }

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public int TimeTakenInMinutes { get; set; }

  [Required]
  public List<MockInterviewRoundDto> MockInterviewRoundDtos { get; set; } = new();
}

public record MockInterviewRoundDto
{
  public LeetcodeMockInterviewRoundDto? LeetcodeMockInterviewRound { get; set; }
  public BehaviouralMockInterviewRoundDto? BehaviouralMockInterviewRound { get; set; }
  public CustomMockInterviewRoundDto? CustomMockInterviewRound { get; set; }

  // For update
  public string? MockInterviewRoundId { get; set; }
}

public record LeetcodeMockInterviewRoundDto
{
  [Required]
  public int ConfirmQuestionScore { get; set; }

  [Required]
  public int AlgorithmDesignScore { get; set; }

  [Required]
  public int ComplexityAnalysisScore { get; set; }

  [Required]
  public int CodingScore { get; set; }

  [Required]
  public int TestingScore { get; set; }

  [Required]
  public string LeetcodeProblemId { get; set; } = string.Empty;
}

public record BehaviouralMockInterviewRoundDto
{
  [Required]
  public int BehavioralScore { get; set; }
}

public record CustomMockInterviewRoundDto
{
  [Required]
  public string Content { get; set; } = string.Empty;

  [Required]
  public string Link { get; set; } = string.Empty;

  [Required]
  public int Score { get; set; }
}

public class CreateMockInterviewResponse { }
