using System.Net;
using System.Reflection;
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
    public Guid InterviewerUserId { get; set; }
    public string IntervieweeEmail { get; set; }
    public Guid? EnrollmentId { get; set; }
    public DateTime StartDate { get; set; }
    public int TimeTakenInMinutes { get; set; }
    public List<MockInterviewRoundDto> MockInterviewRoundDtos { get; set; }
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

    private bool IsAllScoresAboveThreshold(Command request)
    {
      const int threshold = 5;

      var numList = new List<int>();
      foreach (var round in request.MockInterviewRoundDtos)
      {
        if (round.LeetcodeMockInterviewRound != null)
        {
          var r = round.LeetcodeMockInterviewRound;
          numList.AddRange(new List<int>
          {
            r.ConfirmQuestionScore,
            r.AlgorithmDesignScore,
            r.ComplexityAnalysisScore,
            r.CodingScore, 
            r.TestingScore
          });
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
        if (num < threshold) return false;
      }
      
      return true;
    }

    public async Task<ApiResult<CreateMockInterviewResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        // Check if the enrollment exists
        if (request.EnrollmentId != null)
        {
          var existingEnrollment = await _dbContext
                                         .Enrollments
                                         .Include(e => e.User)
                                         .FirstOrDefaultAsync(e => e.EnrollmentId == request.EnrollmentId, cancellationToken);
          if (existingEnrollment == null || existingEnrollment.User.Email != request.IntervieweeEmail)
          {
            return new ApiResult<CreateMockInterviewResponse>
            {
              StatusCode = HttpStatusCode.BadRequest,
              Error = new ApiError(Message.EnrollmentDoesNotExists)
            };
          }
        }
        
        // Get user for interviewee and interviewer
        var interviewee = await _dbContext
                                       .Users
                                       .Where(u => u.Email == request.IntervieweeEmail)
                                       .Select(u => new User
                                       {
                                         UserId = u.UserId
                                       })
                                       .FirstOrDefaultAsync(cancellationToken);
        if (interviewee == null)
        {
          return new ApiResult<CreateMockInterviewResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.UserEmailDoesNotExists)
          };
        }
        
        var interviewer = await _dbContext
                                .Users
                                .Where(u => u.UserId == request.InterviewerUserId)
                                .Select(u => new User
                                {
                                  UserId = u.UserId
                                })
                                .FirstOrDefaultAsync(cancellationToken);
        if (interviewer == null)
        {
          return new ApiResult<CreateMockInterviewResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.UserEmailDoesNotExists)
          };
        }
        
        // Process each mock interview round
        var mockInterviewRounds = new List<MockInterviewRound>();
        foreach (var m in request.MockInterviewRoundDtos)
        {
          // TODO: Store notes
          var mockInterviewRound = new MockInterviewRound
          {
            IsReviewedByInterviewee = false,
            IntervieweeComment = ""
          };
          
          // Order: Behavioural -> Leetcode -> Custom and there can only be one
          if (m.BehaviouralMockInterviewRound != null)
          {
            var round = m.BehaviouralMockInterviewRound;
            mockInterviewRound.BehaviouralMockInterviewRound = new BehaviouralMockInterviewRound
            {
              BehavioralScore = round.BehavioralScore
            };
            m.LeetcodeMockInterviewRound = null;
            m.CustomMockInterviewRound = null;
          }
          
          if (m.LeetcodeMockInterviewRound != null)
          {
            var round = m.LeetcodeMockInterviewRound;
            mockInterviewRound.LeetcodeMockInterviewRound = new LeetcodeMockInterviewRound
            {
              ConfirmQuestionScore = round.ConfirmQuestionScore,
              AlgorithmDesignScore = round.AlgorithmDesignScore,
              ComplexityAnalysisScore = round.ComplexityAnalysisScore, 
              CodingScore = round.CodingScore, 
              TestingScore = round.TestingScore,
              LeetcodeProblemId = round.LeetcodeProblemId
            };
            m.CustomMockInterviewRound = null;
          }
          
          if (m.CustomMockInterviewRound != null)
          {
            var round = m.CustomMockInterviewRound;
            mockInterviewRound.CustomMockInterviewRound = new CustomMockInterviewRound
            {
              Content = round.Content,
              Score = round.Score,
              Link = round.Link
            };
          }
          
          mockInterviewRounds.Add(mockInterviewRound);
        }
        
        var mockInterview = new MockInterview
        {
          IsPass = IsAllScoresAboveThreshold(request),
          InterviewerUserId = interviewer.UserId,
          IntervieweeUserId = interviewee.UserId,
          EnrollmentId = request.EnrollmentId,
          MockInterviewRounds = mockInterviewRounds,
          StartDate = request.StartDate,
          TimeTakenInMinutes = request.TimeTakenInMinutes,
        };
        
        _dbContext.Add(mockInterview);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<CreateMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.MockInterviewCreatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MockInterviewCreationUnexpectedError);

        return new ApiResult<CreateMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MockInterviewCreationUnexpectedError)
        };
      }
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
             TimeTakenInMinutes = request.TimeTakenInMinutes
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
  public Guid InterviewerUserId { get; set; }
  public Guid? EnrollmentId { get; set; }
  public DateTime StartDate { get; set; }
  public int TimeTakenInMinutes { get; set; }
  public List<MockInterviewRoundDto> MockInterviewRoundDtos { get; set; }
}

public record MockInterviewRoundDto
{
  public Guid? MockInterviewRoundId { get; set; }
  public LeetcodeMockInterviewRoundDto? LeetcodeMockInterviewRound { get; set; }
  public BehaviouralMockInterviewRoundDto? BehaviouralMockInterviewRound { get; set; }
  public CustomMockInterviewRoundDto? CustomMockInterviewRound { get; set; }
}

public record LeetcodeMockInterviewRoundDto
{
  public int ConfirmQuestionScore { get; set; }
  public int AlgorithmDesignScore { get; set; }
  public int ComplexityAnalysisScore { get; set; }
  public int CodingScore { get; set; }
  public int TestingScore { get; set; }
  public Guid LeetcodeProblemId { get; set; }
}

public record BehaviouralMockInterviewRoundDto
{
  public int BehavioralScore { get; set; }
}

public record CustomMockInterviewRoundDto
{
  public string Content { get; set; }
  public string Link { get; set; }
  public int Score { get; set; }
}

public class CreateMockInterviewResponse
{
}