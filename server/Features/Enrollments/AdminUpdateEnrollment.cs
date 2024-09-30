using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Enrollments;

public static class AdminUpdateEnrollment
{
  public class Command : AdminAuthRequest<ApiResult<AdminUpdateEnrollmentResponse>>
  {
    public Guid EnrollmentId { get; set; }
    public Guid SeasonId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.EnrollmentId).NotEmpty();
      RuleFor(c => c.SeasonId).NotEmpty();
      RuleFor(c => c.UserId).NotEmpty();
      RuleFor(c => c.RoleId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminUpdateEnrollmentResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminUpdateEnrollmentResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingEnrollment = await _dbContext
                                     .Enrollments
                                     .FirstOrDefaultAsync(u => u.EnrollmentId == request.EnrollmentId,
                                                          cancellationToken);
      if (existingEnrollment == null)
      {
        return new ApiResult<AdminUpdateEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.EnrollmentDoesNotExists)
        };
      }

      existingEnrollment.SeasonId = request.SeasonId;
      existingEnrollment.UserId = request.UserId;
      existingEnrollment.RoleId = request.RoleId;

      try
      {
        _dbContext.Update(existingEnrollment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminUpdateEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminUpdateEnrollmentResponse
          {
            EnrollmentId = existingEnrollment.EnrollmentId,
            SeasonId = existingEnrollment.SeasonId,
            UserId = existingEnrollment.UserId,
            RoleId = existingEnrollment.RoleId
          },
          SuccessMessage = Message.EnrollmentUpdatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.EnrollmentUpdateUnexpectedError);

        return new ApiResult<AdminUpdateEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.EnrollmentUpdateUnexpectedError)
        };
      }
    }
  }
}

public class AdminUpdateEnrollmentEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPut(
         "api/admin/enrollments",
         async (AdminUpdateEnrollmentRequest request, ISender sender) =>
         {
           var command = new AdminUpdateEnrollment.Command
           {
             EnrollmentId = request.EnrollmentId,
             SeasonId = request.SeasonId,
             UserId = request.UserId,
             RoleId = request.RoleId
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminUpdateEnrollment");
  }
}

public record AdminUpdateEnrollmentRequest
{
  public Guid EnrollmentId { get; set; }
  public Guid SeasonId { get; set; }
  public Guid UserId { get; set; }
  public Guid RoleId { get; set; }
}

public class AdminUpdateEnrollmentResponse
{
  public Guid EnrollmentId { get; set; }
  public Guid SeasonId { get; set; }
  public Guid UserId { get; set; }
  public Guid RoleId { get; set; }
}