using System.Net;
using Microsoft.EntityFrameworkCore;
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

public class UpdateMockInterviewTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<UpdateMockInterview.Handler>> _loggerMock;

  public UpdateMockInterviewTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<UpdateMockInterview.Handler>>();
  }

  private UpdateMockInterview.Command CreateDummyCommand()
  {
    return new UpdateMockInterview.Command
    {
      MockInterviewId = DummyId1,
      InterviewerUserId = DummyId2,
      IntervieweeEmail = DummyEmail,
      EnrollmentId = DummyId3,
      StartDate = DummyStartDate,
      TimeTakenInMinutes = 60,
      MockInterviewRoundDtos = new List<MockInterviewRoundDto>
      {
        new MockInterviewRoundDto
        {
          MockInterviewRoundId = DummyId3,
          BehaviouralMockInterviewRound = new BehaviouralMockInterviewRoundDto
          {
            BehavioralScore = 8,
          },
          LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundDto
          {
            ConfirmQuestionScore = 7,
            AlgorithmDesignScore = 9,
            ComplexityAnalysisScore = 8,
            CodingScore = 7,
            TestingScore = 6,
            LeetcodeProblemId = DummyId2,
          },
        },
      },
    };
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenMockInterviewIsUpdatedWithEnrollment()
  {
    var interviewer = new UserEntity { UserId = DummyId2 };
    var interviewee = new UserEntity { UserId = DummyId3, Email = DummyEmail };
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
      EnrollmentId = DummyId3,
      User = interviewee,
      Season = season,
      SeasonId = season.SeasonId,
    };
    var existingMockInterview = new MockInterviewEntity
    {
      MockInterviewId = DummyId1,
      InterviewerUserId = interviewer.UserId,
      EnrollmentId = enrollment.EnrollmentId,
      Enrollment = enrollment,
      MockInterviewRounds = new List<MockInterviewRoundEntity>
      {
        new MockInterviewRoundEntity
        {
          MockInterviewRoundId = DummyId3,
          BehaviouralMockInterviewRound = new BehaviouralMockInterviewRoundEntity
          {
            BehavioralScore = 6,
          },
          LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundEntity
          {
            ConfirmQuestionScore = 5,
            AlgorithmDesignScore = 6,
            ComplexityAnalysisScore = 5,
            CodingScore = 6,
            TestingScore = 5,
            LeetcodeProblemId = DummyId2,
          },
        },
      },
    };

    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { seasonWeek });
    _dbContextMock
      .Setup(x => x.MockInterviews)
      .ReturnsDbSet(new List<MockInterviewEntity> { existingMockInterview });
    _dbContextMock
      .Setup(x => x.Users)
      .ReturnsDbSet(new List<UserEntity> { interviewer, interviewee });

    var handler = new UpdateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MockInterviewUpdatedSuccessfully, result.SuccessMessage);

    var mockInterviewRounds = existingMockInterview.MockInterviewRounds.ToList();
    Assert.Equal(8, mockInterviewRounds[0].BehaviouralMockInterviewRound.BehavioralScore);
    Assert.Equal(7, mockInterviewRounds[0].LeetcodeMockInterviewRound.ConfirmQuestionScore);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WhenMockInterviewIsUpdatedWithoutEnrollment()
  {
    var existingMockInterview = new MockInterviewEntity
    {
      MockInterviewId = DummyId1,
      InterviewerUserId = DummyId2,
      MockInterviewRounds = new List<MockInterviewRoundEntity>
      {
        new MockInterviewRoundEntity
        {
          MockInterviewRoundId = DummyId3,
          BehaviouralMockInterviewRound = new BehaviouralMockInterviewRoundEntity
          {
            BehavioralScore = 6,
          },
          LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundEntity
          {
            ConfirmQuestionScore = 5,
            AlgorithmDesignScore = 6,
            ComplexityAnalysisScore = 5,
            CodingScore = 6,
            TestingScore = 5,
            LeetcodeProblemId = DummyId2,
          },
        },
      },
    };

    var interviewer = new UserEntity { UserId = DummyId2 };
    var interviewee = new UserEntity { UserId = DummyId3, Email = DummyEmail };

    _dbContextMock
      .Setup(x => x.MockInterviews)
      .ReturnsDbSet(new List<MockInterviewEntity> { existingMockInterview });
    _dbContextMock
      .Setup(x => x.Users)
      .ReturnsDbSet(new List<UserEntity> { interviewer, interviewee });

    var handler = new UpdateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    command.EnrollmentId = null;
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MockInterviewUpdatedSuccessfully, result.SuccessMessage);

    var mockInterviewRounds = existingMockInterview.MockInterviewRounds.ToList();
    Assert.Equal(8, mockInterviewRounds[0].BehaviouralMockInterviewRound.BehavioralScore);
    Assert.Equal(7, mockInterviewRounds[0].LeetcodeMockInterviewRound.ConfirmQuestionScore);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenMockInterviewDoesNotExist()
  {
    _dbContextMock.Setup(x => x.MockInterviews).ReturnsDbSet(new List<MockInterviewEntity>());

    var handler = new UpdateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MockInterviewDoesNotExists, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenInterviewerDoesNotExist()
  {
    var existingMockInterview = new MockInterviewEntity
    {
      MockInterviewId = DummyId1,
      IntervieweeUserId = DummyId3,
    };

    var interviewee = new UserEntity { UserId = DummyId3, Email = DummyEmail };

    _dbContextMock
      .Setup(x => x.MockInterviews)
      .ReturnsDbSet(new List<MockInterviewEntity> { existingMockInterview });
    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { interviewee });

    var handler = new UpdateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    command.EnrollmentId = null;
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailDoesNotExists, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenIntervieweeDoesNotExist()
  {
    var existingMockInterview = new MockInterviewEntity
    {
      MockInterviewId = DummyId1,
      InterviewerUserId = DummyId2,
    };

    var interviewer = new UserEntity { UserId = DummyId2 };

    _dbContextMock
      .Setup(x => x.MockInterviews)
      .ReturnsDbSet(new List<MockInterviewEntity> { existingMockInterview });
    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { interviewer });

    var handler = new UpdateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    command.EnrollmentId = null;
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailDoesNotExists, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenCurrentDateOutsideOfSeasonWeekRange()
  {
    var interviewer = new UserEntity { UserId = DummyId2 };
    var interviewee = new UserEntity { UserId = DummyId3, Email = DummyEmail };
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
      EnrollmentId = DummyId3,
      User = interviewee,
      Season = season,
      SeasonId = season.SeasonId,
    };
    var existingMockInterview = new MockInterviewEntity
    {
      MockInterviewId = DummyId1,
      InterviewerUserId = DummyId2,
      EnrollmentId = enrollment.EnrollmentId,
      Enrollment = enrollment,
      MockInterviewRounds = new List<MockInterviewRoundEntity>
      {
        new MockInterviewRoundEntity
        {
          MockInterviewRoundId = DummyId3,
          BehaviouralMockInterviewRound = new BehaviouralMockInterviewRoundEntity
          {
            BehavioralScore = 6,
          },
          LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundEntity
          {
            ConfirmQuestionScore = 5,
            AlgorithmDesignScore = 6,
            ComplexityAnalysisScore = 5,
            CodingScore = 6,
            TestingScore = 5,
            LeetcodeProblemId = DummyId2,
          },
        },
      },
    };

    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { seasonWeek });
    _dbContextMock
      .Setup(x => x.MockInterviews)
      .ReturnsDbSet(new List<MockInterviewEntity> { existingMockInterview });
    _dbContextMock
      .Setup(x => x.Users)
      .ReturnsDbSet(new List<UserEntity> { interviewer, interviewee });

    var handler = new UpdateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MockInterviewOutOfSeasonDateRange, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
