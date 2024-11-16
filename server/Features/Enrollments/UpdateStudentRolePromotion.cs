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

public static class UpdateStudentRolePromotion
{
  public class Command : AuthRequest<ApiResult<UpdateStudentRolePromotionResponse>>
  {
    public string MenteeEnrollmentId { get; set; } = string.Empty;
    public string SeasonSlug { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.MenteeEnrollmentId).NotEmpty();
      RuleFor(c => c.SeasonSlug).NotEmpty();
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
      RuleFor(c => c.StudentRolePromotion).IsInEnum();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<UpdateStudentRolePromotionResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<UpdateStudentRolePromotionResponse>> Handle(
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
        return new ApiResult<UpdateStudentRolePromotionResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.UpdateStudentRolePromotionEnrollmentDoesNotExists),
        };
      }

      // If the current user is a mentor, ensure the mentorship exists
      if (existingEnrollment.Role == SeasonRole.Mentor)
      {
        var existingMentorship = await _dbContext.Mentorships.FirstOrDefaultAsync(
          m =>
            m.MentorEnrollment.User.Email == request.Email
            && m.MenteeEnrollmentId == request.MenteeEnrollmentId,
          cancellationToken
        );
        if (existingMentorship == null)
        {
          return new ApiResult<UpdateStudentRolePromotionResponse>
          {
            StatusCode = HttpStatusCode.BadRequest,
            Error = new ApiError(Message.MentorshipDoesNotExists),
          };
        }
      }

      // Ensure the student we want to kick is an actual student
      var studentEnrollment = await _dbContext.Enrollments.FirstOrDefaultAsync(
        e => e.EnrollmentId == request.MenteeEnrollmentId && e.Role == SeasonRole.Student,
        cancellationToken
      );
      if (studentEnrollment == null)
      {
        return new ApiResult<UpdateStudentRolePromotionResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.UpdateStudentRolePromotionMenteeDoesntExist),
        };
      }

      if (request.StudentRolePromotion == SeasonStudentRolePromotion.NotApplicable)
      {
        return new ApiResult<UpdateStudentRolePromotionResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.EnrollmentStudentMustHaveAppropriateRolePromotion),
        };
      }

      studentEnrollment.StudentRolePromotion = request.StudentRolePromotion;

      try
      {
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<UpdateStudentRolePromotionResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.UpdateStudentRolePromotionSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.UpdateStudentRolePromotionUnexpectedError);

        return new ApiResult<UpdateStudentRolePromotionResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.UpdateStudentRolePromotionUnexpectedError),
        };
      }
    }
  }
}

public class UpdateStudentRolePromotionEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
        "enrollments/update-student-role-promotion",
        async (
          UpdateStudentRolePromotionRequest request,
          ISender sender,
          HttpContext httpContext
        ) =>
        {
          var email = httpContext?.User?.Identity?.Name ?? "";

          var command = new UpdateStudentRolePromotion.Command
          {
            MenteeEnrollmentId = request.MenteeEnrollmentId,
            SeasonSlug = request.SeasonSlug,
            StudentRolePromotion = request.StudentRolePromotion,
            Email = email,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("UpdateStudentRolePromotion");
  }
}

public class UpdateStudentRolePromotionRequest
{
  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string SeasonSlug { get; set; } = string.Empty;

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}

public class UpdateStudentRolePromotionResponse { }
