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

namespace RSPWebAPI.Features.ProblemAttempts;

public static class CreateProblemAttempt
{
  public class Command : AuthRequest<ApiResult<CreateProblemAttemptResponse>>
  {
    public DateTime AttemptStartDateUtc { get; set; }
    public int TimeTakenInMinutes { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? LeetcodeProblemId { get; set; }
    public string? CustomProblemId { get; set; }
    public string? EnrollmentId { get; set; }
    public string Email { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.AttemptStartDateUtc).NotEmpty();
      RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThanOrEqualTo(1);
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<CreateProblemAttemptResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<CreateProblemAttemptResponse>> Handle(
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
          if (existingEnrollment == null || existingEnrollment.User.Email != request.Email)
          {
            return new ApiResult<CreateProblemAttemptResponse>
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
            return new ApiResult<CreateProblemAttemptResponse>
            {
              StatusCode = HttpStatusCode.BadRequest,
              Error = new ApiError(Message.ProblemAttemptOutOfSeasonDateRange),
            };
          }
        }

        // Leetcode should take precedence if CustomProblem is present for some reason
        if (request.CustomProblemId != null && request.LeetcodeProblemId != null)
        {
          request.CustomProblemId = null;
        }

        // Get user. At this point, it should have exists based on authentication middleware
        var existingUser = await _dbContext
          .Users.Where(u => u.Email == request.Email)
          .Select(u => new UserEntity { UserId = u.UserId })
          .FirstOrDefaultAsync(cancellationToken);
        if (existingUser == null)
        {
          return new ApiResult<CreateProblemAttemptResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.UserEmailDoesNotExists),
          };
        }

        var problemAttempt = new ProblemAttemptEntity
        {
          ProblemAttemptId = Database.Constants.GeneratePrimaryKeyId(),
          AttemptStartDateUtc = request.AttemptStartDateUtc,
          TimeTakenInMinutes = request.TimeTakenInMinutes,
          LeetcodeProblemId = request.LeetcodeProblemId,
          CustomProblemId = request.CustomProblemId,
          Notes = request.Notes,
          EnrollmentId = request.EnrollmentId,
          UserId = existingUser.UserId,
          SeasonWeekId = seasonWeekId,
        };

        _dbContext.Add(problemAttempt);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<CreateProblemAttemptResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.ProblemAttemptCreatedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.ProblemAttemptCreationUnexpectedError);

        return new ApiResult<CreateProblemAttemptResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.ProblemAttemptCreationUnexpectedError),
        };
      }
    }
  }
}

public class CreateProblemAttemptEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
        "api/problem-attempts",
        async (CreateProblemAttemptRequest request, ISender sender, HttpContext httpContext) =>
        {
          var email = httpContext?.User?.Identity?.Name ?? "";

          var command = new CreateProblemAttempt.Command
          {
            AttemptStartDateUtc = request.AttemptStartDateUtc,
            TimeTakenInMinutes = request.TimeTakenInMinutes,
            Notes = request.Notes,
            Email = email,
            LeetcodeProblemId = request.LeetcodeProblemId,
            CustomProblemId = request.CustomProblemId,
            EnrollmentId = request.EnrollmentId,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("CreateProblemAttempt");
  }
}

public record CreateProblemAttemptRequest
{
  [Required]
  public DateTime AttemptStartDateUtc { get; set; }

  [Required]
  public int TimeTakenInMinutes { get; set; }

  [Required]
  public string Notes { get; set; } = string.Empty;
  public string LeetcodeProblemId { get; set; } = string.Empty;
  public string CustomProblemId { get; set; } = string.Empty;
  public string? EnrollmentId { get; set; }
}

public class CreateProblemAttemptResponse { }
