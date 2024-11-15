using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.ProblemAttempts;

public static class DeleteProblemAttempt
{
  public class Command : AuthRequest<ApiResult<DeleteProblemAttemptResponse>>
  {
    public string ProblemAttemptId { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.ProblemAttemptId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<DeleteProblemAttemptResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<DeleteProblemAttemptResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingProblemAttempt = await _dbContext
                                         .ProblemAttempts.FirstOrDefaultAsync(
                                           u => u.ProblemAttemptId == request.ProblemAttemptId, cancellationToken);
      if (existingProblemAttempt == null)
      {
        return new ApiResult<DeleteProblemAttemptResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.ProblemAttemptDoesNotExists)
        };
      }

      try
      {
        _dbContext.Remove(existingProblemAttempt);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<DeleteProblemAttemptResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.ProblemAttemptDeletedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.ProblemAttemptDeletionUnexpectedError);

        return new ApiResult<DeleteProblemAttemptResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.ProblemAttemptDeletionUnexpectedError)
        };
      }
    }
  }
}

public class DeleteProblemAttemptEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete(
         "api/problem-attempts",
         async (string problemAttemptId, ISender sender) =>
         {
           var command = new DeleteProblemAttempt.Command
           {
             ProblemAttemptId = problemAttemptId
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("DeleteProblemAttempt");
  }
}

public class DeleteProblemAttemptResponse
{
}
