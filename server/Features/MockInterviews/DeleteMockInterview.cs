using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.MockInterviews;

public static class DeleteMockInterview
{
  public class Command : AuthRequest<ApiResult<DeleteMockInterviewResponse>>
  {
    public string MockInterviewId { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.MockInterviewId).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<DeleteMockInterviewResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<DeleteMockInterviewResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var existingMockInterview = await _dbContext
        .MockInterviews.Where(u => u.MockInterviewId == request.MockInterviewId)
        .FirstOrDefaultAsync(cancellationToken);
      if (existingMockInterview == null)
      {
        return new ApiResult<DeleteMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError(Message.MockInterviewDoesNotExists),
        };
      }

      try
      {
        _dbContext.Remove(existingMockInterview);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<DeleteMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.OK,
          SuccessMessage = Message.MockInterviewDeletedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.MockInterviewDeletionUnexpectedError);

        return new ApiResult<DeleteMockInterviewResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.MockInterviewDeletionUnexpectedError),
        };
      }
    }
  }
}

public class DeleteMockInterviewEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete(
        "api/mock-interviews",
        async (string mockInterviewId, ISender sender) =>
        {
          var command = new DeleteMockInterview.Command { MockInterviewId = mockInterviewId };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("DeleteMockInterview");
  }
}

public class DeleteMockInterviewResponse { }
