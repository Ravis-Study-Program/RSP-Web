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

public class UpdateProblemAttemptTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<UpdateProblemAttempt.Handler>> _loggerMock;

  public UpdateProblemAttemptTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<UpdateProblemAttempt.Handler>>();
  }

  private UpdateProblemAttempt.Command UpdateDummyCommand()
  {
    return new UpdateProblemAttempt.Command
    {
      ProblemAttemptId = DummyId3,
      AttemptStartDateUtc = DummyStartDate,
      TimeTakenInMinutes = 30,
      Notes = "Attempt notes",
      Email = DummyEmail,
      LeetcodeProblemId = DummyId1,
      EnrollmentId = DummyId2,
    };
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenProblemAttemptIsUpdatedWithoutEnrollment()
  {
    var user = new UserEntity { UserId = DummyId1, Email = DummyEmail };
    var enrollment = new EnrollmentEntity { EnrollmentId = DummyId2, User = user };
    var problemAttempt = new ProblemAttemptEntity
    {
      AttemptStartDateUtc = DummyEndDate,
      EnrollmentId = enrollment.EnrollmentId,
      UserId = user.UserId,
      ProblemAttemptId = DummyId3,
    };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { user });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock
      .Setup(x => x.ProblemAttempts)
      .ReturnsDbSet(new List<ProblemAttemptEntity> { problemAttempt });

    var handler = new UpdateProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = UpdateDummyCommand();
    command.EnrollmentId = null;

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptUpdatedSuccessfully, result.SuccessMessage);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenProblemAttemptIsUpdatedWithEnrollment()
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
    var problemAttempt = new ProblemAttemptEntity
    {
      AttemptStartDateUtc = DummyEndDate,
      EnrollmentId = enrollment.EnrollmentId,
      UserId = user.UserId,
      ProblemAttemptId = DummyId3,
      Enrollment = enrollment,
    };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { user });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { seasonWeek });
    _dbContextMock
      .Setup(x => x.ProblemAttempts)
      .ReturnsDbSet(new List<ProblemAttemptEntity> { problemAttempt });

    var handler = new UpdateProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = UpdateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptUpdatedSuccessfully, result.SuccessMessage);
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
    var problemAttempt = new ProblemAttemptEntity
    {
      AttemptStartDateUtc = DummyEndDate,
      EnrollmentId = enrollment.EnrollmentId,
      UserId = user.UserId,
      ProblemAttemptId = DummyId3,
      Enrollment = enrollment,
    };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { user });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { seasonWeek });
    _dbContextMock
      .Setup(x => x.ProblemAttempts)
      .ReturnsDbSet(new List<ProblemAttemptEntity> { problemAttempt });

    var handler = new UpdateProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = UpdateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptOutOfSeasonDateRange, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenEnrollmentDoesNotExist()
  {
    var user = new UserEntity { UserId = DummyId1, Email = DummyEmail };
    var problemAttempt = new ProblemAttemptEntity
    {
      AttemptStartDateUtc = DummyEndDate,
      UserId = user.UserId,
      ProblemAttemptId = DummyId3,
    };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { user });
    _dbContextMock
      .Setup(x => x.ProblemAttempts)
      .ReturnsDbSet(new List<ProblemAttemptEntity> { problemAttempt });

    var handler = new UpdateProblemAttempt.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = UpdateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptDoesNotExists, result.Error?.Message);
  }
}
