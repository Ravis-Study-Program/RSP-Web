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

public static class AdminCreateEnrollment
{
  public class Command : AdminAuthRequest<ApiResult<AdminCreateEnrollmentResponse>>
  {
    public string SeasonId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public SeasonRole Role { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.SeasonId).NotEmpty();
      RuleFor(c => c.UserId).NotEmpty();
      RuleFor(c => c.Role).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminCreateEnrollmentResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminCreateEnrollmentResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingEnrollment = await _dbContext
                                     .Enrollments
                                     .FirstOrDefaultAsync(
                                       e => e.SeasonId == request.SeasonId && e.UserId == request.UserId && e.Role ==
                                            request.Role,
                                       cancellationToken);
      if (existingEnrollment != null)
      {
        return new ApiResult<AdminCreateEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.EnrollmentExists)
        };
      }

      var enrollment = new EnrollmentEntity
      {
        EnrollmentId = Database.Constants.GeneratePrimaryKeyId(),
        SeasonId = request.SeasonId,
        UserId = request.UserId,
        Role = request.Role
      };

      try
      {
        _dbContext.Add(enrollment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminCreateEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminCreateEnrollmentResponse
          {
            SeasonId = enrollment.SeasonId,
            UserId = enrollment.UserId,
            EnrollmentId = enrollment.EnrollmentId,
            Role = enrollment.Role
          },
          SuccessMessage = Message.EnrollmentCreatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.EnrollmentCreationUnexpectedError);

        return new ApiResult<AdminCreateEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.EnrollmentCreationUnexpectedError)
        };
      }
    }
  }
}

public class AdminCreateEnrollmentEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
         "api/admin/enrollments",
         async (AdminCreateEnrollmentRequest request, ISender sender) =>
         {
           var command = new AdminCreateEnrollment.Command
           {
             SeasonId = request.SeasonId,
             UserId = request.UserId,
             Role = request.Role
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminCreateEnrollment");
  }
}

public record AdminCreateEnrollmentRequest
{
  public string SeasonId { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
  public SeasonRole Role { get; set; }
}

public class AdminCreateEnrollmentResponse
{
  public string EnrollmentId { get; set; } = string.Empty;
  public string SeasonId { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
  public SeasonRole Role { get; set; }
}
