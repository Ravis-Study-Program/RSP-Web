using System.Net;
using Microsoft.EntityFrameworkCore;
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

public class GetProblemAttemptsTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<GetProblemAttempts.Handler>> _loggerMock;

  public GetProblemAttemptsTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<GetProblemAttempts.Handler>>();
  }

  private GetProblemAttempts.Command CreateDummyCommand()
  {
    return new GetProblemAttempts.Command
    {
      Email = DummyEmail,
      EnrollmentId = DummyId1,
      IncludeLeetcode = true,
      IncludeCustom = false,
    };
  }

  [Fact]
  public async Task Handle_ShouldReturnSuccess_WithProblemAttempts()
  {
    var problemAttempt = new ProblemAttemptEntity
    {
      ProblemAttemptId = DummyId1,
      EnrollmentId = DummyId1,
      UserId = DummyId1,
      User = new UserEntity { Email = DummyEmail },
      Notes = "Test notes",
      TimeTakenInMinutes = 30,
      LeetcodeProblem = new LeetcodeProblemEntity
      {
        Problem = new ProblemEntity { Title = "Two Sum" },
      },
    };

    var enrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      User = new UserEntity { Email = DummyEmail },
    };

    var user = enrollment.User;

    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { enrollment });
    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { user });
    _dbContextMock
      .Setup(x => x.ProblemAttempts)
      .ReturnsDbSet(new List<ProblemAttemptEntity> { problemAttempt });

    var handler = new GetProblemAttempts.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();
    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.ProblemAttemptListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.ProblemAttempts);
    Assert.Single(result.ResponseBody.ProblemAttempts);
    Assert.Equal("Test notes", result.ResponseBody.ProblemAttempts[0].Notes);
    Assert.Equal("Two Sum", result.ResponseBody.ProblemAttempts[0].LeetcodeProblem.Problem.Title);
  }

  [Fact]
  public async Task Handle_ShouldReturnError_WhenEnrollmentDoesNotExist()
  {
    var problemAttempt = new ProblemAttemptEntity
    {
      ProblemAttemptId = DummyId1,
      EnrollmentId = DummyId1,
      UserId = DummyId1,
    };

    _dbContextMock.Setup(x => x.Enrollments).ReturnsDbSet(new List<EnrollmentEntity> { });
    _dbContextMock
      .Setup(x => x.ProblemAttempts)
      .ReturnsDbSet(new List<ProblemAttemptEntity> { problemAttempt });

    var handler = new GetProblemAttempts.Handler(_dbContextMock.Object, _loggerMock.Object);
    var command = CreateDummyCommand();

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.EnrollmentDoesNotExists, result.Error?.Message);
  }
}
