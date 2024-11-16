using System.ComponentModel.DataAnnotations;
using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Migrations;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Mentorships;

public static class GetCurrentUserMenteesList
{
  public class Command : AuthRequest<ApiResult<GetCurrentUserMenteesListResponse>>
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

  public class Handler : IRequestHandler<Command, ApiResult<GetCurrentUserMenteesListResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<GetCurrentUserMenteesListResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      try
      {
        var mentees = await _dbContext
          .Mentorships.Where(m =>
            m.MentorEnrollment.User.Email == request.Email
            && m.MentorEnrollment.Season.Slug == request.SeasonSlug
          )
          .Select(m => new MenteeResponseDto
          {
            MenteeName = m.MenteeEnrollment.User.Name,
            MenteeEnrollmentId = m.MenteeEnrollment.EnrollmentId,
            StudentRolePromotion = m.MenteeEnrollment.StudentRolePromotion,
          })
          .AsNoTracking()
          .ToListAsync(cancellationToken);

        return new ApiResult<GetCurrentUserMenteesListResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.MentorshipListSuccessfully,
          ResponseBody = new GetCurrentUserMenteesListResponse { Mentees = mentees },
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MentorshipListUnexpectedError);

        return new ApiResult<GetCurrentUserMenteesListResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MentorshipListUnexpectedError),
        };
      }
    }
  }
}

public class GetCurrentUserMenteesListEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
        "api/mentorships/get-current-user-mentees-list/{seasonSlug}",
        async (string seasonSlug, ISender sender, HttpContext httpContext) =>
        {
          var email = httpContext?.User?.Identity?.Name ?? "";

          var command = new GetCurrentUserMenteesList.Command
          {
            Email = email,
            SeasonSlug = seasonSlug,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("GetCurrentUserMenteesList");
  }
}

public record MenteeResponseDto
{
  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string MenteeName { get; set; } = string.Empty;

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}

public record GetCurrentUserMenteesListResponse
{
  [Required]
  public List<MenteeResponseDto> Mentees { get; set; } = new();
}
