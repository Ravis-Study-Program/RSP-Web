using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
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
      var existingMentorship = await _dbContext
                                     .Mentorships
                                     .FirstOrDefaultAsync(u => u.MentorshipId == request.MentorshipId,
                                                          cancellationToken);
      if (existingMentorship == null)
      {
        return new ApiResult<AdminUpdateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipDoesNotExists)
        };
      }

      // Reject if any of the provided mentor or mentee doesn't exist
      var mentor = await _dbContext
                         .Enrollments
                         .Include(e => e.Season)
                         .FirstOrDefaultAsync(e => e.EnrollmentId == request.MentorEnrollmentId, cancellationToken);
      var mentee = await _dbContext
                         .Enrollments
                         .Include(e => e.Season)
                         .FirstOrDefaultAsync(e => e.EnrollmentId == request.MenteeEnrollmentId, cancellationToken);

      if (mentor == null || mentee == null)
      {
        return new ApiResult<AdminUpdateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipNotPermittedDueToNullMentorOrMentee)
        };
      }

      // Reject if provided mentor and mentee doesn't belong to the same season
      if (mentor.Season.SeasonId != mentee.Season.SeasonId)
      {
        return new ApiResult<AdminUpdateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipNotPermittedDueToDifferentSeason)
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
            MenteeEnrollmentId = existingMentorship.MenteeEnrollmentId
          },
          SuccessMessage = Message.MentorshipUpdatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MentorshipUpdateUnexpectedError);

        return new ApiResult<AdminUpdateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MentorshipUpdateUnexpectedError)
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
             MenteeEnrollmentId = request.MenteeEnrollmentId
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
  public string MentorshipId { get; set; } = string.Empty;
  public string MentorEnrollmentId { get; set; } = string.Empty;
  public string MenteeEnrollmentId { get; set; } = string.Empty;
}

public class AdminUpdateMentorshipResponse
{
  public string MentorshipId { get; set; } = string.Empty;
  public string MentorEnrollmentId { get; set; } = string.Empty;
  public string MenteeEnrollmentId { get; set; } = string.Empty;
}
