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

public static class GetCurrentUserEnrollments
{
  public class Command : AuthRequest<ApiResult<GetCurrentUserEnrollmentsResponse>>
  {
    public string Email { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<GetCurrentUserEnrollmentsResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetCurrentUserEnrollmentsResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var enrollments = await _dbContext
                                .Enrollments
                                .Where(e => e.User.Email == request.Email)
                                .Select(e => new EnrollmentResponseDto
                                {
                                  SeasonSlug = e.Season.Slug,
                                  SeasonName = e.Season.Name,
                                  SeasonImageUrl = e.Season.ImageUrl,
                                  Role = e.Role
                                })
                                .AsNoTracking()
                                .ToListAsync(cancellationToken);

        return new ApiResult<GetCurrentUserEnrollmentsResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new GetCurrentUserEnrollmentsResponse
          {
            Enrollments = enrollments
          }
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.EnrollmentListSuccessfully);

        return new ApiResult<GetCurrentUserEnrollmentsResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.EnrollmentListSuccessfully)
        };
      }
    }
  }
}

public class GetCurrentUserEnrollmentsEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
         "api/enrollments/get-current-user-enrollments",
         async (ISender sender, HttpContext httpContext) =>
         {
           var email = httpContext?.User?.Identity?.Name ?? "";

           var command = new GetCurrentUserEnrollments.Command
           {
             Email = email
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("GetCurrentUserEnrollments");
  }
}

public record EnrollmentResponseDto
{
  [Required] public string SeasonName { get; set; } = string.Empty;
  [Required] public string SeasonImageUrl { get; set; } = string.Empty;
  [Required] public string SeasonSlug { get; set; } = string.Empty;
  [Required] public SeasonRole Role { get; set; }
}

public record GetCurrentUserEnrollmentsResponse
{
  [Required] public IList<EnrollmentResponseDto> Enrollments { get; set; } = new List<EnrollmentResponseDto>();
}
