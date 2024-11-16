using System.ComponentModel.DataAnnotations;
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
  public class Command : AdminAuthRequest<ApiResult<AdminListEnrollmentResponse>> { }

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
        var enrollments = await _dbContext
          .Enrollments.Select(e => new EnrollmentResponse
          {
            EnrollmentId = e.EnrollmentId,
            Role = e.Role,
            SeasonId = e.Season.SeasonId,
            SeasonName = e.Season.Name,
            UserId = e.User.UserId,
            UserName = e.User.Name,
          })
          .AsNoTracking()
          .ToListAsync(cancellationToken);

        return new ApiResult<AdminListEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.EnrollmentListSuccessfully,
          ResponseBody = new AdminListEnrollmentResponse { Enrollments = enrollments },
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.EnrollmentListUnexpectedError);

        return new ApiResult<AdminListEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.EnrollmentListUnexpectedError),
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

public class EnrollmentResponse
{
  [Required]
  public string EnrollmentId { get; set; } = string.Empty;

  [Required]
  public SeasonRole Role { get; set; }

  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string SeasonName { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public string UserName { get; set; } = string.Empty;
}

public class AdminListEnrollmentResponse
{
  [Required]
  public ICollection<EnrollmentResponse> Enrollments { get; set; } = new List<EnrollmentResponse>();
}
