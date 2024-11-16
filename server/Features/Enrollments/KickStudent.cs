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

namespace RSPWebAPI.Features.Enrollments;

public static class KickStudent
{
  public class Command : AuthRequest<ApiResult<KickStudentResponse>>
  {
    public string MenteeEnrollmentId { get; set; } = string.Empty;
    public string SeasonSlug { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.MenteeEnrollmentId).NotEmpty();
      RuleFor(c => c.SeasonSlug).NotEmpty();
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<KickStudentResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<KickStudentResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      // Ensure that the current user is enrolled in the season and is a mentor or coordinator
      var existingEnrollment = await _dbContext.Enrollments.FirstOrDefaultAsync(
        e =>
          e.Season.Slug == request.SeasonSlug
          && e.User.Email == request.Email
          && (e.Role == SeasonRole.Mentor || e.Role == SeasonRole.Coordinator),
        cancellationToken
      );
      if (existingEnrollment == null)
      {
        return new ApiResult<KickStudentResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.KickStudentCurrentUserEnrollmentDoesNotExists),
        };
      }

      // Ensure the student we want to kick is an actual student
      var studentEnrollment = await _dbContext.Enrollments.FirstOrDefaultAsync(
        e => e.EnrollmentId == request.MenteeEnrollmentId && e.Role == SeasonRole.Student,
        cancellationToken
      );
      if (studentEnrollment == null)
      {
        return new ApiResult<KickStudentResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.KickStudentMenteeDoesntExist),
        };
      }

      try
      {
        _dbContext.Enrollments.Remove(studentEnrollment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<KickStudentResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.KickStudentSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.KickStudentUnexpectedError);

        return new ApiResult<KickStudentResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.KickStudentUnexpectedError),
        };
      }
    }
  }
}

public class KickStudentEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
        "enrollments/kick-student",
        async (KickStudentRequest request, ISender sender, HttpContext httpContext) =>
        {
          var email = httpContext?.User?.Identity?.Name ?? "";

          var command = new KickStudent.Command
          {
            MenteeEnrollmentId = request.MenteeEnrollmentId,
            SeasonSlug = request.SeasonSlug,
            Email = email,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("KickStudent");
  }
}

public class KickStudentRequest
{
  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string SeasonSlug { get; set; } = string.Empty;
}

public class KickStudentResponse { }
