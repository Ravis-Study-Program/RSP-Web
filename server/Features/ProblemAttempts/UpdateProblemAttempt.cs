using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.ProblemAttempts;

public static class UpdateProblemAttempt
{
  public class Command : AuthRequest<ApiResult<UpdateProblemAttemptResponse>>
  {
    public string ProblemAttemptId { get; set; } = string.Empty;
    public DateTime AttemptStartDate { get; set; }
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
      RuleFor(c => c.ProblemAttemptId).NotEmpty();
      RuleFor(c => c.AttemptStartDate).NotEmpty();
      RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThanOrEqualTo(1);
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<UpdateProblemAttemptResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<UpdateProblemAttemptResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        // Check if the problemAttempt exists and if it matches with the enrollment ID
        var existingProblemAttempt = await _dbContext
                                           .ProblemAttempts
                                           .Include(p => p.Enrollment)
                                           .FirstOrDefaultAsync(e => e.ProblemAttemptId == request.ProblemAttemptId,
                                                                cancellationToken);
        if (existingProblemAttempt == null || (request.EnrollmentId != null &&
                                               existingProblemAttempt.Enrollment?.EnrollmentId != request.EnrollmentId))
        {
          return new ApiResult<UpdateProblemAttemptResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.ProblemAttemptDoesNotExists)
          };
        }

        // Leetcode should take precedence if CustomProblem is present for some reason
        if (request.CustomProblemId != null && request.LeetcodeProblemId != null)
        {
          request.CustomProblemId = null;
        }

        existingProblemAttempt.AttemptStartDateUtc = request.AttemptStartDate;
        existingProblemAttempt.TimeTakenInMinutes = request.TimeTakenInMinutes;
        existingProblemAttempt.LeetcodeProblemId = request.LeetcodeProblemId;
        existingProblemAttempt.CustomProblemId = request.CustomProblemId;
        existingProblemAttempt.Notes = request.Notes;

        _dbContext.Update(existingProblemAttempt);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<UpdateProblemAttemptResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.ProblemAttemptUpdatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.ProblemAttemptCreationUnexpectedError);

        return new ApiResult<UpdateProblemAttemptResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.ProblemAttemptCreationUnexpectedError)
        };
      }
    }
  }
}

public class UpdateProblemAttemptEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPut(
         "api/problem-attempts",
         async (UpdateProblemAttemptRequest request, ISender sender, HttpContext httpContext) =>
         {
           var email = httpContext?.User?.Identity?.Name ?? "";

           var command = new UpdateProblemAttempt.Command
           {
             ProblemAttemptId = request.ProblemAttemptId,
             AttemptStartDate = request.AttemptStartDate,
             TimeTakenInMinutes = request.TimeTakenInMinutes,
             Notes = request.Notes,
             Email = email,
             LeetcodeProblemId = request.LeetcodeProblemId,
             CustomProblemId = request.CustomProblemId,
             EnrollmentId = request.EnrollmentId
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("UpdateProblemAttempt");
  }
}

public record UpdateProblemAttemptRequest
{
  public string ProblemAttemptId { get; set; } = string.Empty;
  public DateTime AttemptStartDate { get; set; }
  public int TimeTakenInMinutes { get; set; }
  public string Notes { get; set; } = string.Empty;
  public string LeetcodeProblemId { get; set; } = string.Empty;
  public string CustomProblemId { get; set; } = string.Empty;
  public string? EnrollmentId { get; set; }
}

public class UpdateProblemAttemptResponse
{
}
