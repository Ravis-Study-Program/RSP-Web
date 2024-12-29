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

public class CreateProblemAttemptTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<CreateProblemAttempt.Handler>> _loggerMock;

  public CreateProblemAttemptTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<CreateProblemAttempt.Handler>>();
  }

  private CreateProblemAttempt.Command CreateDummyCommand()
  {
    return new CreateProblemAttempt.Command
    {
      AttemptStartDateUtc = DummyStartDate,
      TimeTakenInMinutes = 30,
      Notes = "Attempt notes",
      Email = DummyEmail,
      LeetcodeProblemId = DummyId1,
      EnrollmentId = DummyId2,
    };
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenProblemAttemptIsCreatedWithEnrollment()
  {
    var user = new UserEntity { UserId = DummyId1, Email = DummyEmail };
    var season = new SeasonEntity
    {
      SeasonId = DummyId3,
      StartDateInclusiveUtc = DateTime.UtcNow.AddDays(-30),
      EndDateInclusiveUtc = DateTime.UtcNow.AddDays(30),
    };
    var seasonWeek = new SeasonWeekEntity
    {
      SeasonId = season.SeasonId,
      SeasonWeekId = DummyId1,
      StartDate = DateTime.UtcNow.AddDays(-5),
      EndDate = DateTime.UtcNow.AddDays(2),
    };
    var enrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      User = user,
      Season = season,
      SeasonId = season.SeasonId,
    };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { user });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { seasonWeek });

    var handler = new CreateProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptCreatedSuccessfully, result.SuccessMessage);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenProblemAttemptIsCreatedWithoutEnrollment()
  {
    var user = new UserEntity { UserId = DummyId1, Email = DummyEmail };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { user });

    var handler = new CreateProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    command.EnrollmentId = null;

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptCreatedSuccessfully, result.SuccessMessage);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenCurrentDateOutsideOfSeasonWeekRange()
  {
    var user = new UserEntity { UserId = DummyId1, Email = DummyEmail };
    var season = new SeasonEntity
    {
      SeasonId = DummyId3,
      StartDateInclusiveUtc = DateTime.UtcNow.AddDays(-30),
      EndDateInclusiveUtc = DateTime.UtcNow.AddDays(30),
    };
    var seasonWeek = new SeasonWeekEntity
    {
      SeasonId = season.SeasonId,
      SeasonWeekId = DummyId1,
      StartDate = DateTime.UtcNow.AddDays(-20),
      EndDate = DateTime.UtcNow.AddDays(-13),
    };
    var enrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      User = user,
      Season = season,
      SeasonId = season.SeasonId,
    };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { user });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { seasonWeek });

    var handler = new CreateProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptOutOfSeasonDateRange, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenEnrollmentDoesNotExist()
  {
    var user = new UserEntity { UserId = DummyId1, Email = DummyEmail };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { user });
    _dbContextMock.Setup(x => x.Enrollments).ReturnsDbSet(new List<EnrollmentEntity>());

    var handler = new CreateProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.EnrollmentDoesNotExists, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenUserDoesNotExist()
  {
    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity>());
    _dbContextMock.Setup(x => x.Enrollments).ReturnsDbSet(new List<EnrollmentEntity>());

    var handler = new CreateProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    command.EnrollmentId = null;

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailDoesNotExists, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
