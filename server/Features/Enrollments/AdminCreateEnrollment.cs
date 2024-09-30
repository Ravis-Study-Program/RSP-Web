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
    public Guid SeasonId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.SeasonId).NotEmpty();
      RuleFor(c => c.UserId).NotEmpty();
      RuleFor(c => c.RoleId).NotEmpty();
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
                                     .FirstOrDefaultAsync(u => u.SeasonId == request.SeasonId && u.UserId == request.UserId && u.RoleId == request.RoleId,
                                                          cancellationToken);
      if (existingEnrollment != null)
      {
        return new ApiResult<AdminCreateEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.EnrollmentExists)
        };
      }
      
      var enrollment = new Enrollment
      {
        SeasonId = request.SeasonId,
        UserId = request.UserId,
        RoleId = request.RoleId
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
            RoleId = enrollment.RoleId,
            EnrollmentId = enrollment.EnrollmentId
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
             RoleId = request.RoleId
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
  public Guid SeasonId { get; set; }
  public Guid UserId { get; set; }
  public Guid RoleId { get; set; }
}

public class AdminCreateEnrollmentResponse
{
  public Guid EnrollmentId { get; set; }
  public Guid SeasonId { get; set; }
  public Guid UserId { get; set; }
  public Guid RoleId { get; set; }
}