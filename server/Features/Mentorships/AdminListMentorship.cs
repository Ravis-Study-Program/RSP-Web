using System.Net;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
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
                                          .Include(m => m.MentorEnrollment)
                                          .ThenInclude(m => m.User)
                                          .Include(m => m.MentorEnrollment)
                                          .ThenInclude(m => m.Season)
                                          .Include(m => m.MenteeEnrollment)
                                          .ThenInclude(m => m.User)
                                          .Select(m => new MentorshipResponse
                                          {
                                            MentorshipId = m.MentorshipId,
                                            Mentor = m.MentorEnrollment,
                                            Mentee = m.MenteeEnrollment,
                                            Season = m.MentorEnrollment.Season
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
  public string MentorshipId { get; set; } = string.Empty;
  public SeasonEntity? Season { get; set; }
  public EnrollmentEntity? Mentor { get; set; }
  public EnrollmentEntity? Mentee { get; set; }
}

public class AdminListMentorshipResponse
{
  public ICollection<MentorshipResponse> Mentorships { get; set; } = new List<MentorshipResponse>();
}
