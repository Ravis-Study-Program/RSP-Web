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

public class CreateMockInterviewTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<CreateMockInterview.Handler>> _loggerMock;

  public CreateMockInterviewTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<CreateMockInterview.Handler>>();
  }

  private CreateMockInterview.Command CreateDummyCommand()
  {
    return new CreateMockInterview.Command
    {
      InterviewerUserId = DummyId1,
      IntervieweeEmail = DummyEmail,
      EnrollmentId = DummyId2,
      StartDate = DummyStartDate,
      TimeTakenInMinutes = 30,
      MockInterviewRoundDtos = new List<MockInterviewRoundDto>
      {
        new MockInterviewRoundDto
        {
          LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundDto
          {
            ConfirmQuestionScore = 7,
            AlgorithmDesignScore = 8,
            ComplexityAnalysisScore = 9,
            CodingScore = 7,
            TestingScore = 6,
            LeetcodeProblemId = DummyId3,
          },
        },
      },
    };
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenMockInterviewIsCreatedWithoutEnrollment()
  {
    var interviewer = new UserEntity { UserId = DummyId1, Email = "interviewer@example.com" };
    var interviewee = new UserEntity { UserId = DummyId2, Email = DummyEmail };

    _dbContextMock
      .Setup(x => x.Users)
      .ReturnsDbSet(new List<UserEntity> { interviewer, interviewee });

    var handler = new CreateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    command.EnrollmentId = null;

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MockInterviewCreatedSuccessfully, result.SuccessMessage);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenMockInterviewIsCreatedWithEnrollment()
  {
    var interviewer = new UserEntity { UserId = DummyId1, Email = "interviewer@example.com" };
    var interviewee = new UserEntity { UserId = DummyId2, Email = DummyEmail };
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
      User = interviewee,
      Season = season,
      SeasonId = season.SeasonId,
    };

    _dbContextMock
      .Setup(x => x.Users)
      .ReturnsDbSet(new List<UserEntity> { interviewer, interviewee });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { seasonWeek });

    var handler = new CreateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MockInterviewCreatedSuccessfully, result.SuccessMessage);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenCurrentDateOutsideOfSeasonWeekRange()
  {
    var interviewer = new UserEntity { UserId = DummyId1, Email = "interviewer@example.com" };
    var interviewee = new UserEntity { UserId = DummyId2, Email = DummyEmail };
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
      User = interviewee,
      Season = season,
      SeasonId = season.SeasonId,
    };

    _dbContextMock
      .Setup(x => x.Users)
      .ReturnsDbSet(new List<UserEntity> { interviewer, interviewee });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { seasonWeek });

    var handler = new CreateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MockInterviewOutOfSeasonDateRange, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenEnrollmentDoesNotExist()
  {
    var interviewer = new UserEntity { UserId = DummyId1, Email = "interviewer@example.com" };
    var interviewee = new UserEntity { UserId = DummyId2, Email = DummyEmail };

    _dbContextMock
      .Setup(x => x.Users)
      .ReturnsDbSet(new List<UserEntity> { interviewer, interviewee });
    _dbContextMock.Setup(x => x.Enrollments).ReturnsDbSet(new List<EnrollmentEntity>());

    var handler = new CreateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.EnrollmentDoesNotExists, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenIntervieweeDoesNotExist()
  {
    var interviewer = new UserEntity { UserId = DummyId1, Email = DummyEmail };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { interviewer });

    var handler = new CreateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    command.EnrollmentId = null;
    command.IntervieweeEmail = "dummy@gmail.com";

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailDoesNotExists, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenInterviewerDoesNotExist()
  {
    var interviewee = new UserEntity { UserId = DummyId2, Email = DummyEmail };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { interviewee });

    var handler = new CreateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    command.EnrollmentId = null;

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailDoesNotExists, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
