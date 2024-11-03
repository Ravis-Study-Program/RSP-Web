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

public static class GetMockInterviews
{
  public class Command : AuthRequest<ApiResult<GetMockInterviewsResponse>>
  {
    public string Email { get; set; } = string.Empty;
    public string? EnrollmentId { get; set; }
    public bool IncludeBehavioural { get; set; }
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

  public class Handler : IRequestHandler<Command, ApiResult<GetMockInterviewsResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetMockInterviewsResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var query = _dbContext.MockInterviews.AsQueryable();

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
            return new ApiResult<GetMockInterviewsResponse>
            {
              StatusCode = HttpStatusCode.BadRequest,
              Error = new ApiError(Message.EnrollmentDoesNotExists)
            };
          }

          query = query.Where(p => p.EnrollmentId == request.EnrollmentId);
        }

        if (request.IncludeBehavioural)
        {
          query = query
                  .Include(m => m.MockInterviewRounds)
                  .ThenInclude(mr => mr.BehaviouralMockInterviewRound);
        }

        if (request.IncludeLeetcode)
        {
          query = query
                  .Include(m => m.MockInterviewRounds)
                  .ThenInclude(mr => mr.LeetcodeMockInterviewRound)
                  .ThenInclude(l => l.LeetcodeProblem)
                  .ThenInclude(l => l.Problem);
        }

        if (request.IncludeCustom)
        {
          query = query
                  .Include(m => m.MockInterviewRounds)
                  .ThenInclude(mr => mr.CustomMockInterviewRound);
        }

        var result = await query
                           .Include(m => m.Interviewer)
                           .ToListAsync(cancellationToken);

        return new ApiResult<GetMockInterviewsResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new GetMockInterviewsResponse
          {
            MockInterviews = result
          },
          SuccessMessage = Message.MockInterviewListSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MockInterviewListUnexpectedError);

        return new ApiResult<GetMockInterviewsResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MockInterviewListUnexpectedError)
        };
      }
    }
  }
}

public class GetMockInterviewsEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/mock-interviews",
         async (string? enrollmentId, bool includeLeetcode, bool includeCustom, bool includeBehavioural, ISender sender,
           HttpContext httpContext) =>
         {
           var email = httpContext?.User?.Identity?.Name ?? "";

           var command = new GetMockInterviews.Command
           {
             EnrollmentId = enrollmentId,
             Email = email,
             IncludeLeetcode = includeLeetcode,
             IncludeCustom = includeCustom,
             IncludeBehavioural = includeBehavioural
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("GetMockInterviews");
  }
}

public class GetMockInterviewsResponse
{
  public IList<MockInterviewEntity> MockInterviews { get; set; } = new List<MockInterviewEntity>();
}
