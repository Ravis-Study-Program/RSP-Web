using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Leetcode;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Recommender
{
  public class GenerateLeetcodeProblemRecommendationsTests : TestsHelper
  {
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public GenerateLeetcodeProblemRecommendationsTests()
    {
      _dbContextMock = new Mock<ApplicationDbContext>();
    }

    private GenerateLeetcodeProblemRecommendations.Command CreateDummyCommand()
    {
      return new GenerateLeetcodeProblemRecommendations.Command { UserId = DummyId1 };
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRecommendationIsCreated()
    {
      _dbContextMock
        .Setup(x => x.LeetcodeProblemRecommendations)
        .ReturnsDbSet(new List<LeetcodeProblemRecommendationEntity>());
      _dbContextMock.Setup(x => x.ProblemAttempts).ReturnsDbSet(new List<ProblemAttemptEntity>());
      _dbContextMock
        .Setup(x => x.LeetcodeProblems)
        .ReturnsDbSet(
          new List<LeetcodeProblemEntity>
          {
            new LeetcodeProblemEntity { LeetcodeProblemId = DummyId2 },
          }
        );

      var handler = new GenerateLeetcodeProblemRecommendations.Handler(_dbContextMock.Object);
      var command = CreateDummyCommand();

      var result = await handler.Handle(command, CancellationToken.None);

      Assert.Equal(HttpStatusCode.OK, result.StatusCode);
      Assert.Equal(Message.LeetcodeProblemRecommenderCreatedSuccessfully, result.SuccessMessage);
      _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenNoProblemsLeft()
    {
      _dbContextMock
        .Setup(x => x.LeetcodeProblemRecommendations)
        .ReturnsDbSet(new List<LeetcodeProblemRecommendationEntity>());
      _dbContextMock
        .Setup(x => x.ProblemAttempts)
        .ReturnsDbSet(
          new List<ProblemAttemptEntity>
          {
            new ProblemAttemptEntity { LeetcodeProblemId = DummyId2, UserId = DummyId1 },
          }
        );
      _dbContextMock
        .Setup(x => x.LeetcodeProblems)
        .ReturnsDbSet(
          new List<LeetcodeProblemEntity>
          {
            new LeetcodeProblemEntity { LeetcodeProblemId = DummyId2 },
          }
        );

      var handler = new GenerateLeetcodeProblemRecommendations.Handler(_dbContextMock.Object);
      var command = CreateDummyCommand();

      var result = await handler.Handle(command, CancellationToken.None);

      Assert.Equal(HttpStatusCode.OK, result.StatusCode);
      Assert.Equal(Message.LeetcodeProblemRecommenderNoLeetcodeProblemLeft, result.SuccessMessage);
      _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenExistingRecommendationExists()
    {
      _dbContextMock
        .Setup(x => x.LeetcodeProblemRecommendations)
        .ReturnsDbSet(
          new List<LeetcodeProblemRecommendationEntity>
          {
            new LeetcodeProblemRecommendationEntity
            {
              UserId = DummyId1,
              LeetcodeProblemRecommendationId = DummyId2,
              ProblemAttemptId = null,
              LeetcodeProblemId = DummyId3,
            },
          }
        );
      _dbContextMock.Setup(x => x.ProblemAttempts).ReturnsDbSet(new List<ProblemAttemptEntity>());

      var handler = new GenerateLeetcodeProblemRecommendations.Handler(_dbContextMock.Object);
      var command = CreateDummyCommand();

      var result = await handler.Handle(command, CancellationToken.None);

      Assert.Equal(HttpStatusCode.OK, result.StatusCode);
      Assert.Equal(Message.LeetcodeProblemRecommenderCreatedSuccessfully, result.SuccessMessage);
      _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
  }
}
