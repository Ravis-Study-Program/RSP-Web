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

namespace RSPWebAPI.Features.Mentorships;

public static class AdminCreateMentorship
{
  public class Command : AdminAuthRequest<ApiResult<AdminCreateMentorshipResponse>>
  {
    public string MentorEnrollmentId { get; set; } = string.Empty;
    public string MenteeEnrollmentId { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.MentorEnrollmentId).NotEmpty();
      RuleFor(c => c.MenteeEnrollmentId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminCreateMentorshipResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminCreateMentorshipResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      // Reject if any of the provided mentor or mentee doesn't exist
      var mentorSeasonId = await _dbContext
        .Enrollments.Where(e =>
          e.EnrollmentId == request.MentorEnrollmentId && e.Role == SeasonRole.Mentor
        )
        .Select(e => e.Season.SeasonId)
        .FirstOrDefaultAsync(cancellationToken);

      var menteeSeasonId = await _dbContext
        .Enrollments.Where(e =>
          e.EnrollmentId == request.MenteeEnrollmentId && e.Role == SeasonRole.Student
        )
        .Select(e => e.Season.SeasonId)
        .FirstOrDefaultAsync(cancellationToken);

      if (mentorSeasonId == null || menteeSeasonId == null)
      {
        return new ApiResult<AdminCreateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipNotPermittedDueToNullMentorOrMentee),
        };
      }

      // Reject if provided mentor and mentee doesn't belong to the same season
      if (mentorSeasonId != menteeSeasonId)
      {
        return new ApiResult<AdminCreateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipNotPermittedDueToDifferentSeason),
        };
      }

      // Reject if there is an existing mentorship
      var existingMentorship = await _dbContext.Mentorships.FirstOrDefaultAsync(
        m =>
          m.MentorEnrollmentId == request.MentorEnrollmentId
          && m.MenteeEnrollmentId == request.MenteeEnrollmentId,
        cancellationToken
      );
      if (existingMentorship != null)
      {
        return new ApiResult<AdminCreateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipExists),
        };
      }

      var mentorship = new MentorshipEntity
      {
        MentorshipId = Database.Constants.GeneratePrimaryKeyId(),
        MentorEnrollmentId = request.MentorEnrollmentId,
        MenteeEnrollmentId = request.MenteeEnrollmentId,
      };

      try
      {
        _dbContext.Add(mentorship);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminCreateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminCreateMentorshipResponse
          {
            MentorshipId = mentorship.MentorshipId,
            MentorEnrollmentId = mentorship.MentorEnrollmentId,
            MenteeEnrollmentId = mentorship.MenteeEnrollmentId,
          },
          SuccessMessage = Message.MentorshipCreatedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MentorshipCreationUnexpectedError);

        return new ApiResult<AdminCreateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MentorshipCreationUnexpectedError),
        };
      }
    }
  }
}

public class AdminCreateMentorshipEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
        "api/admin/mentorships",
        async (AdminCreateMentorshipRequest request, ISender sender) =>
        {
          var command = new AdminCreateMentorship.Command
          {
            MentorEnrollmentId = request.MentorEnrollmentId,
            MenteeEnrollmentId = request.MenteeEnrollmentId,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminCreateMentorship");
  }
}

public record AdminCreateMentorshipRequest
{
  [Required]
  public string MentorEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;
}

public class AdminCreateMentorshipResponse
{
  [Required]
  public string MentorshipId { get; set; } = string.Empty;

  [Required]
  public string MentorEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;
}
