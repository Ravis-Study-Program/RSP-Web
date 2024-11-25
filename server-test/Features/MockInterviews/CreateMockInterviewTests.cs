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
  public async Task Handle_ShouldReturnSuccess_WhenMockInterviewIsCreated()
  {
    var interviewer = new UserEntity { UserId = DummyId1, Email = "interviewer@example.com" };
    var interviewee = new UserEntity { UserId = DummyId2, Email = DummyEmail };
    var enrollment = new EnrollmentEntity { EnrollmentId = DummyId2, User = interviewee };

    _dbContextMock
      .Setup(x => x.Users)
      .ReturnsDbSet(new List<UserEntity> { interviewer, interviewee });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });

    var handler = new CreateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MockInterviewCreatedSuccessfully, result.SuccessMessage);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
    var enrollment = new EnrollmentEntity { EnrollmentId = DummyId2, User = interviewer };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { interviewer });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });

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
    var enrollment = new EnrollmentEntity { EnrollmentId = DummyId2, User = interviewee };

    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { interviewee });
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });

    var handler = new CreateMockInterview.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailDoesNotExists, result.Error?.Message);
    _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
