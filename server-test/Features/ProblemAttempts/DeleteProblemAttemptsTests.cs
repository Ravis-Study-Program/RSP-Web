using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.ProblemAttempts;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.ProblemAttempts;

public class DeleteProblemAttemptTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<DeleteProblemAttempt.Handler>> _loggerMock;

  public DeleteProblemAttemptTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<DeleteProblemAttempt.Handler>>();
  }

  private DeleteProblemAttempt.Command CreateDummyCommand()
  {
    return new DeleteProblemAttempt.Command { ProblemAttemptId = DummyId1 };
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenProblemAttemptIsDeleted()
  {
    var problemAttemptId = DummyId1;
    var problemAttempt = new ProblemAttemptEntity { ProblemAttemptId = problemAttemptId };

    _dbContextMock
      .Setup(x => x.ProblemAttempts)
      .ReturnsDbSet(new List<ProblemAttemptEntity> { problemAttempt });

    var handler = new DeleteProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = new DeleteProblemAttempt.Command { ProblemAttemptId = problemAttemptId };

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptDeletedSuccessfully, result.SuccessMessage);
    _dbContextMock.Verify(x => x.Remove(problemAttempt), Times.Once);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenProblemAttemptDoesNotExist()
  {
    _dbContextMock.Setup(x => x.ProblemAttempts).ReturnsDbSet(new List<ProblemAttemptEntity>());

    var handler = new DeleteProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptDoesNotExists, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
