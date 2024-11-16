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

public static class AdminUpdateMentorship
{
  public class Command : AdminAuthRequest<ApiResult<AdminUpdateMentorshipResponse>>
  {
    public string MentorshipId { get; set; } = string.Empty;
    public string MentorEnrollmentId { get; set; } = string.Empty;
    public string MenteeEnrollmentId { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.MentorshipId).NotEmpty();
      RuleFor(c => c.MentorEnrollmentId).NotEmpty();
      RuleFor(c => c.MenteeEnrollmentId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminUpdateMentorshipResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminUpdateMentorshipResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingMentorship = await _dbContext.Mentorships.FirstOrDefaultAsync(
        u => u.MentorshipId == request.MentorshipId,
        cancellationToken
      );
      if (existingMentorship == null)
      {
        return new ApiResult<AdminUpdateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipDoesNotExists),
        };
      }

      // Reject if any of the newly provided mentor or mentee doesn't exist
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
        return new ApiResult<AdminUpdateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipNotPermittedDueToNullMentorOrMentee),
        };
      }

      // Reject if newly provided mentor and mentee doesn't belong to the same season
      if (mentorSeasonId != menteeSeasonId)
      {
        return new ApiResult<AdminUpdateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipNotPermittedDueToDifferentSeason),
        };
      }

      existingMentorship.MentorEnrollmentId = request.MentorEnrollmentId;
      existingMentorship.MenteeEnrollmentId = request.MenteeEnrollmentId;

      try
      {
        _dbContext.Update(existingMentorship);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminUpdateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminUpdateMentorshipResponse
          {
            MentorshipId = existingMentorship.MentorshipId,
            MentorEnrollmentId = existingMentorship.MentorEnrollmentId,
            MenteeEnrollmentId = existingMentorship.MenteeEnrollmentId,
          },
          SuccessMessage = Message.MentorshipUpdatedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MentorshipUpdateUnexpectedError);

        return new ApiResult<AdminUpdateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MentorshipUpdateUnexpectedError),
        };
      }
    }
  }
}

public class AdminUpdateMentorshipEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPut(
        "api/admin/mentorships",
        async (AdminUpdateMentorshipRequest request, ISender sender) =>
        {
          var command = new AdminUpdateMentorship.Command
          {
            MentorshipId = request.MentorshipId,
            MentorEnrollmentId = request.MentorEnrollmentId,
            MenteeEnrollmentId = request.MenteeEnrollmentId,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminUpdateMentorship");
  }
}

public record AdminUpdateMentorshipRequest
{
  [Required]
  public string MentorshipId { get; set; } = string.Empty;

  [Required]
  public string MentorEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;
}

public class AdminUpdateMentorshipResponse
{
  [Required]
  public string MentorshipId { get; set; } = string.Empty;

  [Required]
  public string MentorEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;
}
