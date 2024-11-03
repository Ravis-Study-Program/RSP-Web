using System.Net;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Enrollments;

public static class AdminListEnrollment
{
  public class Command : AdminAuthRequest<ApiResult<AdminListEnrollmentResponse>>
  {
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminListEnrollmentResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminListEnrollmentResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var enrollments = await _dbContext.Enrollments
                                          .Select(e => new EnrollmentResponse
                                          {
                                            EnrollmentId = e.EnrollmentId,
                                            Role = e.Role,
                                            Season = e.Season.Name,
                                            User = e.User.Name
                                          })
                                          .ToListAsync(cancellationToken);

        return new ApiResult<AdminListEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.EnrollmentListSuccessfully,
          ResponseBody = new AdminListEnrollmentResponse
          {
            Enrollments = enrollments
          }
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.EnrollmentListUnexpectedError);

        return new ApiResult<AdminListEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.EnrollmentListUnexpectedError)
        };
      }
    }
  }
}

public class AdminListEnrollmentEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/admin/enrollments",
         async (ISender sender) =>
         {
           var command = new AdminListEnrollment.Command();
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminListEnrollment");
  }
}

public record EnrollmentResponse
{
  public string EnrollmentId { get; set; } = string.Empty;
  public SeasonRole Role { get; set; }
  public string Season { get; set; } = string.Empty;
  public string User { get; set; } = string.Empty;
}

public class AdminListEnrollmentResponse
{
  public ICollection<EnrollmentResponse> Enrollments { get; set; } = new List<EnrollmentResponse>();
}
