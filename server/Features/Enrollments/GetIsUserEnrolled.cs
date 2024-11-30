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

public static class GetIsUserEnrolled
{
  public class Command : AuthRequest<ApiResult<GetIsUserEnrolledResponse>>
  {
    public string Email { get; set; } = string.Empty;
    public string SeasonSlug { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
      RuleFor(c => c.SeasonSlug).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<GetIsUserEnrolledResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetIsUserEnrolledResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var enrollment = await _dbContext
          .Enrollments.Where(e =>
            e.User.Email == request.Email && e.Season.Slug == request.SeasonSlug
          )
          .Select(e => new GetIsUserEnrolledResponse
          {
            IsEnrolled = true,
            Role = e.Role,
            EnrollmentId = e.EnrollmentId,
            StudentRolePromotion = e.StudentRolePromotion,
            Email = request.Email,
          })
          .AsNoTracking()
          .FirstOrDefaultAsync(cancellationToken);

        return new ApiResult<GetIsUserEnrolledResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody =
            enrollment
            ?? new GetIsUserEnrolledResponse
            {
              IsEnrolled = false,
              Role = null,
              EnrollmentId = null,
              StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
              Email = request.Email,
            },
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.EnrollmentIsUserEnrolledError);

        return new ApiResult<GetIsUserEnrolledResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.EnrollmentIsUserEnrolledError),
        };
      }
    }
  }
}

public class GetIsUserEnrolledEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
        "api/enrollments/get-is-user-enrolled/{seasonSlug}",
        async (string seasonSlug, ISender sender, HttpContext httpContext) =>
        {
          var email = httpContext?.User?.Identity?.Name ?? "";

          var command = new GetIsUserEnrolled.Command { Email = email, SeasonSlug = seasonSlug };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("GetIsUserEnrolled");
  }
}

public record GetIsUserEnrolledResponse
{
  [Required]
  public bool IsEnrolled { get; set; }

  [Required]
  public SeasonRole? Role { get; set; }

  [Required]
  public string? EnrollmentId { get; set; }

  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}
