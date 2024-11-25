using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.MockInterviews;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.MockInterviews;

public class DeleteMockInterviewTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<DeleteMockInterview.Handler>> _loggerMock;

  public DeleteMockInterviewTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<DeleteMockInterview.Handler>>();
  }

  private DeleteMockInterview.Command CreateDummyCommand()
  {
    return new DeleteMockInterview.Command { MockInterviewId = DummyId1 };
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenMockInterviewIsDeleted()
  {
    var existingMockInterview = new MockInterviewEntity { MockInterviewId = DummyId1 };

    _dbContextMock
      .Setup(x => x.MockInterviews)
      .ReturnsDbSet(new List<MockInterviewEntity> { existingMockInterview });

    var handler = new DeleteMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MockInterviewDeletedSuccessfully, result.SuccessMessage);
    _dbContextMock.Verify(x => x.Remove(existingMockInterview), Times.Once);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenMockInterviewDoesNotExist()
  {
    _dbContextMock.Setup(x => x.MockInterviews).ReturnsDbSet(new List<MockInterviewEntity>());

    var handler = new DeleteMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MockInterviewDoesNotExists, result.Error?.Message);
    _dbContextMock.Verify(x => x.Remove(It.IsAny<MockInterviewEntity>()), Times.Never);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
