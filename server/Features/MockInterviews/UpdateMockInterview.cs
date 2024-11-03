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

public static class UpdateMockInterview
{
  public class Command : AuthRequest<ApiResult<UpdateMockInterviewResponse>>
  {
    public string MockInterviewId { get; set; } = string.Empty;
    public string InterviewerUserId { get; set; } = string.Empty;
    public string IntervieweeEmail { get; set; } = string.Empty;
    public string? EnrollmentId { get; set; }
    public DateTime StartDate { get; set; }
    public int TimeTakenInMinutes { get; set; }
    public List<MockInterviewRoundDto> MockInterviewRoundDtos { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.MockInterviewId).NotEmpty();
      RuleFor(c => c.InterviewerUserId).NotEmpty();
      RuleFor(c => c.IntervieweeEmail).NotEmpty().EmailAddress();
      RuleFor(c => c.StartDate).NotEmpty();
      RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThan(0);
      RuleFor(c => c.MockInterviewRoundDtos).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<UpdateMockInterviewResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<UpdateMockInterviewResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        // Check if the mock interview exists
        var existingMockInterview = await _dbContext
                                          .MockInterviews
                                          .Include(m => m.Enrollment)
                                          .Include(m => m.MockInterviewRounds)
                                          .ThenInclude(r => r.BehaviouralMockInterviewRound)
                                          .Include(m => m.MockInterviewRounds)
                                          .ThenInclude(r => r.CustomMockInterviewRound)
                                          .Include(m => m.MockInterviewRounds)
                                          .ThenInclude(r => r.LeetcodeMockInterviewRound)
                                          .FirstOrDefaultAsync(e => e.MockInterviewId == request.MockInterviewId,
                                                               cancellationToken);
        if (existingMockInterview == null || (request.EnrollmentId != null &&
                                              existingMockInterview.Enrollment?.EnrollmentId != request.EnrollmentId))
        {
          return new ApiResult<UpdateMockInterviewResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.MockInterviewDoesNotExists)
          };
        }

        // Get user for interviewee and interviewer
        var interviewee = await _dbContext
                                .Users
                                .Where(u => u.Email == request.IntervieweeEmail)
                                .Select(u => new UserEntity
                                {
                                  UserId = u.UserId
                                })
                                .FirstOrDefaultAsync(cancellationToken);
        if (interviewee == null)
        {
          return new ApiResult<UpdateMockInterviewResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.UserEmailDoesNotExists)
          };
        }

        var interviewer = await _dbContext
                                .Users
                                .Where(u => u.UserId == request.InterviewerUserId)
                                .Select(u => new UserEntity
                                {
                                  UserId = u.UserId
                                })
                                .FirstOrDefaultAsync(cancellationToken);
        if (interviewer == null)
        {
          return new ApiResult<UpdateMockInterviewResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.UserEmailDoesNotExists)
          };
        }

        // Update each mock interview rounds if necessary
        foreach (var round in existingMockInterview.MockInterviewRounds)
        {
          var updatedRoundDto = request.MockInterviewRoundDtos
                                       .FirstOrDefault(dto => dto.MockInterviewRoundId == round.MockInterviewRoundId);

          if (updatedRoundDto == null)
          {
            continue;
          }

          if (round.BehaviouralMockInterviewRound != null && updatedRoundDto.BehaviouralMockInterviewRound != null)
          {
            round.BehaviouralMockInterviewRound.BehavioralScore =
              updatedRoundDto.BehaviouralMockInterviewRound.BehavioralScore;
          }

          if (round.LeetcodeMockInterviewRound != null && updatedRoundDto.LeetcodeMockInterviewRound != null)
          {
            round.LeetcodeMockInterviewRound.ConfirmQuestionScore =
              updatedRoundDto.LeetcodeMockInterviewRound.ConfirmQuestionScore;
            round.LeetcodeMockInterviewRound.AlgorithmDesignScore =
              updatedRoundDto.LeetcodeMockInterviewRound.AlgorithmDesignScore;
            round.LeetcodeMockInterviewRound.ComplexityAnalysisScore =
              updatedRoundDto.LeetcodeMockInterviewRound.ComplexityAnalysisScore;
            round.LeetcodeMockInterviewRound.CodingScore = updatedRoundDto.LeetcodeMockInterviewRound.CodingScore;
            round.LeetcodeMockInterviewRound.TestingScore = updatedRoundDto.LeetcodeMockInterviewRound.TestingScore;
          }

          if (round.CustomMockInterviewRound != null && updatedRoundDto.CustomMockInterviewRound != null)
          {
            round.CustomMockInterviewRound.Score = updatedRoundDto.CustomMockInterviewRound.Score;
            round.CustomMockInterviewRound.Link = updatedRoundDto.CustomMockInterviewRound.Link;
            round.CustomMockInterviewRound.Content = updatedRoundDto.CustomMockInterviewRound.Content;
          }
        }

        // Update fields
        existingMockInterview.IsPass = IsAllScoresAboveThreshold(request);
        existingMockInterview.InterviewerUserId = interviewer.UserId;
        existingMockInterview.IntervieweeUserId = interviewee.UserId;
        existingMockInterview.EnrollmentId = request.EnrollmentId;
        existingMockInterview.StartDate = request.StartDate;
        existingMockInterview.TimeTakenInMinutes = request.TimeTakenInMinutes;

        _dbContext.Update(existingMockInterview);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<UpdateMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.MockInterviewUpdatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MockInterviewCreationUnexpectedError);

        return new ApiResult<UpdateMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MockInterviewCreationUnexpectedError)
        };
      }
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
        if (num < threshold)
        {
          return false;
        }
      }

      return true;
    }
  }
}

public class UpdateMockInterviewEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPut(
         "api/mock-interview",
         async (UpdateMockInterviewRequest request, ISender sender, HttpContext httpContext) =>
         {
           var email = httpContext?.User?.Identity?.Name ?? "";

           var command = new UpdateMockInterview.Command
           {
             MockInterviewId = request.MockInterviewId,
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
       .WithName("UpdateMockInterview");
  }
}

public record UpdateMockInterviewRequest
{
  public string MockInterviewId { get; set; } = string.Empty;
  public string InterviewerUserId { get; set; } = string.Empty;
  public string? EnrollmentId { get; set; }
  public DateTime StartDate { get; set; }
  public int TimeTakenInMinutes { get; set; }
  public List<MockInterviewRoundDto> MockInterviewRoundDtos { get; set; }
}

public class UpdateMockInterviewResponse
{
}
