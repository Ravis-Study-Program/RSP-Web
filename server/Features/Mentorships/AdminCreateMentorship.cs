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
    public Guid MentorEnrollmentId { get; set; }
    public Guid MenteeEnrollmentId { get; set; }
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
        return new ApiResult<AdminCreateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipNotPermittedDueToNullMentorOrMentee)
        };
      }
      
      // Reject if provided mentor and mentee doesn't belong to the same season
      if (mentor.Season.SeasonId != mentee.Season.SeasonId)
      {
        return new ApiResult<AdminCreateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipNotPermittedDueToDifferentSeason)
        };
      }
      
      // Reject if there is an existing mentorship
      var existingMentorship = await _dbContext
                                     .Mentorships
                                     .FirstOrDefaultAsync(m => m.MentorEnrollmentId == request.MentorEnrollmentId && m.MenteeEnrollmentId == request.MenteeEnrollmentId,
                                                          cancellationToken);
      if (existingMentorship != null)
      {
        return new ApiResult<AdminCreateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipExists)
        };
      }
      
      var mentorship = new Mentorship
      {
        MentorEnrollmentId = request.MentorEnrollmentId,
        MenteeEnrollmentId = request.MenteeEnrollmentId
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
            MentorEnrollmentId = request.MentorEnrollmentId,
            MenteeEnrollmentId = request.MenteeEnrollmentId
          },
          SuccessMessage = Message.MentorshipCreatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MentorshipCreationUnexpectedError);

        return new ApiResult<AdminCreateMentorshipResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MentorshipCreationUnexpectedError)
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
             MenteeEnrollmentId = request.MenteeEnrollmentId
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
  public Guid MentorEnrollmentId { get; set; }
  public Guid MenteeEnrollmentId { get; set; }
}

public class AdminCreateMentorshipResponse
{
  public Guid MentorshipId { get; set; }
  public Guid MentorEnrollmentId { get; set; }
  public Guid MenteeEnrollmentId { get; set; }
}
