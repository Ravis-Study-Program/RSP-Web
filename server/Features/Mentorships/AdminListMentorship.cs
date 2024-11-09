using System.ComponentModel.DataAnnotations;
using System.Net;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Mentorships;

public static class AdminListMentorship
{
  public class Command : AdminAuthRequest<ApiResult<AdminListMentorshipResponse>>
  {
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminListMentorshipResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminListMentorshipResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var mentorships = await _dbContext.Mentorships
                                          .Select(m => new MentorshipResponse
                                          {
                                            MentorshipId = m.MentorshipId,
                                            SeasonId = m.MentorEnrollment.SeasonId,
                                            SeasonName = m.MentorEnrollment.Season.Name,
                                            SeasonSlug = m.MenteeEnrollment.Season.Slug,
                                            MentorEnrollmentId = m.MentorEnrollment.EnrollmentId,
                                            MentorName = m.MentorEnrollment.User.Name,
                                            MenteeEnrollmentId = m.MenteeEnrollment.EnrollmentId,
                                            MenteeName = m.MenteeEnrollment.User.Name
                                          })
                                          .ToListAsync(cancellationToken);

        return new ApiResult<AdminListMentorshipResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.MentorshipListSuccessfully,
          ResponseBody = new AdminListMentorshipResponse
          {
            Mentorships = mentorships
          }
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MentorshipListUnexpectedError);

        return new ApiResult<AdminListMentorshipResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MentorshipListUnexpectedError)
        };
      }
    }
  }
}

public class AdminListMentorshipEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/admin/mentorships",
         async (ISender sender) =>
         {
           var command = new AdminListMentorship.Command();
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminListMentorship");
  }
}

public record MentorshipResponse
{
  [Required] public string SeasonId { get; set; } = string.Empty;
  [Required] public string MentorshipId { get; set; } = string.Empty;
  [Required] public string SeasonName { get; set; } = string.Empty;
  [Required] public string SeasonSlug { get; set; } = string.Empty;
  [Required] public string MentorEnrollmentId { get; set; } = string.Empty;
  [Required] public string MentorName { get; set; } = string.Empty;
  [Required] public string MenteeEnrollmentId { get; set; } = string.Empty;
  [Required] public string MenteeName { get; set; } = string.Empty;
}

public class AdminListMentorshipResponse
{
  [Required] public ICollection<MentorshipResponse> Mentorships { get; set; } = new List<MentorshipResponse>();
}
