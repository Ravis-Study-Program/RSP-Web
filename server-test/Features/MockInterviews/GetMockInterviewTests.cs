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

public class GetMockInterviewsTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<GetMockInterviews.Handler>> _loggerMock;

  public GetMockInterviewsTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<GetMockInterviews.Handler>>();
  }

  private GetMockInterviews.Command CreateDummyCommand()
  {
    return new GetMockInterviews.Command
    {
      Email = DummyEmail,
      EnrollmentId = DummyId1,
      IncludeLeetcode = true,
      IncludeCustom = false,
      IncludeBehavioural = true,
    };
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WithMockInterviews()
  {
    var mockInterview = new MockInterviewEntity
    {
      MockInterviewId = DummyId1,
      EnrollmentId = DummyId1,
      Interviewer = new UserEntity { Email = "interviewer@example.com" },
      MockInterviewRounds = new List<MockInterviewRoundEntity>
      {
        new MockInterviewRoundEntity
        {
          BehaviouralMockInterviewRound = new BehaviouralMockInterviewRoundEntity
          {
            BehavioralScore = 8,
          },
          LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundEntity
          {
            ConfirmQuestionScore = 7,
            LeetcodeProblem = new LeetcodeProblemEntity
            {
              Problem = new ProblemEntity { Title = "Two Sum" },
            },
          },
        },
      },
    };

    var enrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      User = new UserEntity { Email = DummyEmail },
    };

    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock
      .Setup(x => x.MockInterviews)
      .ReturnsDbSet(new List<MockInterviewEntity> { mockInterview });

    var handler = new GetMockInterviews.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MockInterviewListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.MockInterviews);

    var mockInterviews = result.ResponseBody.MockInterviews.ToList();
    Assert.Single(mockInterviews);

    var mockInterviewRounds = mockInterviews[0].MockInterviewRounds.ToList();
    Assert.Equal(
      "Two Sum",
      mockInterviewRounds[0].LeetcodeMockInterviewRound.LeetcodeProblem.Problem.Title
    );
    Assert.Equal(8, mockInterviewRounds[0].BehaviouralMockInterviewRound.BehavioralScore);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenEnrollmentDoesNotExist()
  {
    _dbContextMock.Setup(x => x.Enrollments).ReturnsDbSet(new List<EnrollmentEntity>());
    _dbContextMock.Setup(x => x.MockInterviews).ReturnsDbSet(new List<MockInterviewEntity>());

    var handler = new GetMockInterviews.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.EnrollmentDoesNotExists, result.Error?.Message);
  }
}
