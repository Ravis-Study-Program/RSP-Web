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

public static class AdminDeleteEnrollment
{
  public class Command : AdminAuthRequest<ApiResult<AdminDeleteEnrollmentResponse>>
  {
    public string EnrollmentId { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.EnrollmentId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminDeleteEnrollmentResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminDeleteEnrollmentResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingEnrollment = await _dbContext
                                     .Enrollments.FirstOrDefaultAsync(
                                       u => u.EnrollmentId == request.EnrollmentId, cancellationToken);
      if (existingEnrollment == null)
      {
        return new ApiResult<AdminDeleteEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.EnrollmentDoesNotExists)
        };
      }

      try
      {
        _dbContext.Remove(existingEnrollment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminDeleteEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.EnrollmentDeletedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.EnrollmentDeletionUnexpectedError);

        return new ApiResult<AdminDeleteEnrollmentResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.EnrollmentDeletionUnexpectedError)
        };
      }
    }
  }
}

public class AdminDeleteEnrollmentEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete(
         "api/admin/enrollments",
         async (string enrollmentId, ISender sender) =>
         {
           var command = new AdminDeleteEnrollment.Command
           {
             EnrollmentId = enrollmentId
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminDeleteEnrollment");
  }
}

public class AdminDeleteEnrollmentResponse
{
}
