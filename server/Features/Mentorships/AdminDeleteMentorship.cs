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

public static class AdminDeleteMentorship
{
  public class Command : AdminAuthRequest<ApiResult<AdminDeleteMentorshipResponse>>
  {
    public string MentorshipId { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.MentorshipId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminDeleteMentorshipResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminDeleteMentorshipResponse>> Handle(
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
        return new ApiResult<AdminDeleteMentorshipResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MentorshipDoesNotExists),
        };
      }

      try
      {
        _dbContext.Remove(existingMentorship);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminDeleteMentorshipResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.MentorshipDeletedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MentorshipDeletionUnexpectedError);

        return new ApiResult<AdminDeleteMentorshipResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MentorshipDeletionUnexpectedError),
        };
      }
    }
  }
}

public class AdminDeleteMentorshipEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete(
        "api/admin/mentorships",
        async (string mentorshipId, ISender sender) =>
        {
          var command = new AdminDeleteMentorship.Command { MentorshipId = mentorshipId };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminDeleteMentorship");
  }
}

public class AdminDeleteMentorshipResponse { }
