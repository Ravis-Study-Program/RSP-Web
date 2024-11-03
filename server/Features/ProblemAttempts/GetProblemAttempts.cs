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

public static class GetProblemAttempts
{
  public class Command : AuthRequest<ApiResult<GetProblemAttemptsResponse>>
  {
    public string Email { get; set; } = string.Empty;
    public string? EnrollmentId { get; set; }
    public bool IncludeLeetcode { get; set; }
    public bool IncludeCustom { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<GetProblemAttemptsResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetProblemAttemptsResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var query = _dbContext.ProblemAttempts.AsQueryable();

        // Check if the enrollment exists
        if (request.EnrollmentId != null)
        {
          var existingEnrollment = await _dbContext
                                         .Enrollments
                                         .Include(e => e.User)
                                         .FirstOrDefaultAsync(e => e.EnrollmentId == request.EnrollmentId,
                                                              cancellationToken);
          if (existingEnrollment == null || existingEnrollment.User.Email != request.Email)
          {
            return new ApiResult<GetProblemAttemptsResponse>
            {
              StatusCode = HttpStatusCode.BadRequest,
              Error = new ApiError(Message.EnrollmentDoesNotExists)
            };
          }

          query = query.Where(p => p.EnrollmentId == request.EnrollmentId);
        }

        if (request.IncludeLeetcode)
        {
          query = query
                  .Include(p => p.LeetcodeProblem)
                  .ThenInclude(l => l.LeetcodeProblemCategories)
                  .Include(p => p.LeetcodeProblem)
                  .ThenInclude(l => l.LeetcodeProblemDifficulty)
                  .Include(p => p.LeetcodeProblem)
                  .ThenInclude(l => l.Problem);
        }

        if (request.IncludeCustom)
        {
          query = query
                  .Include(p => p.CustomProblem)
                  .ThenInclude(c => c.Problem);
        }

        var result = await query.ToListAsync(cancellationToken);

        return new ApiResult<GetProblemAttemptsResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new GetProblemAttemptsResponse
          {
            ProblemAttempts = result
          },
          SuccessMessage = Message.ProblemAttemptListSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.ProblemAttemptListUnexpectedError);

        return new ApiResult<GetProblemAttemptsResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.ProblemAttemptListUnexpectedError)
        };
      }
    }
  }
}

public class GetProblemAttemptsEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/problem-attempts",
         async (string? enrollmentId, bool includeLeetcode, bool includeCustom, ISender sender,
           HttpContext httpContext) =>
         {
           var email = httpContext?.User?.Identity?.Name ?? "";

           var command = new GetProblemAttempts.Command
           {
             EnrollmentId = enrollmentId,
             Email = email,
             IncludeLeetcode = includeLeetcode,
             IncludeCustom = includeCustom
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("GetProblemAttempts");
  }
}

public record GetProblemAttemptsRequest
{
  public string Email { get; set; } = string.Empty;
  public string? EnrollmentId { get; set; }
  public bool IncludeLeetcode { get; set; } = false;
  public bool IncludeCustom { get; set; } = false;
}

public class GetProblemAttemptsResponse
{
  public IList<ProblemAttemptEntity> ProblemAttempts { get; set; } = new List<ProblemAttemptEntity>();
}
