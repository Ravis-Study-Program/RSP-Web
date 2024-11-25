using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Leetcode;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Leetcode;

public class AdminPopulateLeetcodeQuestionsTests
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<AdminPopulateLeetcodeQuestions.Handler>> _loggerMock;

  public AdminPopulateLeetcodeQuestionsTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<AdminPopulateLeetcodeQuestions.Handler>>();
  }

  private AdminPopulateLeetcodeQuestions.Command DummyCommand() => new();

  [Fact]
  public async Task Handle_Success_OK()
  {
    // Arrange
    var mockResponse = new LeetcodeProblemListApiResponse
    {
      Data = new LeetcodeProblemListApiResponseData
      {
        ProblemsetQuestionList = new LeetcodeProblemList
        {
          Total = 2,
          Questions = new List<LeetcodeProblemListQuestion>
          {
            new()
            {
              Difficulty = "Easy",
              Premium = true,
              QuestionId = "1",
              Title = "Two Sum",
              TitleSlug = "two-sum",
              TopicTags = new List<LeetcodeCategory>
              {
                new() { Name = "Array" },
                new() { Name = "Hash Table" },
              },
            },
            new()
            {
              Difficulty = "Medium",
              Premium = false,
              QuestionId = "2",
              Title = "Add Two Numbers",
              TitleSlug = "add-two-numbers",
              TopicTags = new List<LeetcodeCategory>
              {
                new() { Name = "Linked List" },
                new() { Name = "Math" },
              },
            },
          },
        },
      },
    };

    var serializedResponse = JsonSerializer.Serialize(mockResponse);

    var mockHttpHandler = new MockHttpMessageHandler(
      (request, cancellationToken) =>
      {
        return new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(serializedResponse),
        };
      }
    );

    var client = new HttpClient(mockHttpHandler);

    _dbContextMock
      .Setup(x => x.LeetcodeProblemCategories)
      .ReturnsDbSet(new List<LeetcodeProblemCategoryEntity>());
    _dbContextMock.Setup(x => x.LeetcodeProblems).ReturnsDbSet(new List<LeetcodeProblemEntity>());
    _dbContextMock.Setup(x => x.Problems).ReturnsDbSet(new List<ProblemEntity>());

    var databaseMock = new Mock<DatabaseFacade>(_dbContextMock.Object);
    databaseMock
      .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Mock.Of<IDbContextTransaction>());
    _dbContextMock.Setup(x => x.Database).Returns(databaseMock.Object);

    var command = DummyCommand();
    var handler = new AdminPopulateLeetcodeQuestions.Handler(
      _dbContextMock.Object,
      _loggerMock.Object,
      client
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    _dbContextMock.Verify(
      x => x.LeetcodeProblemCategories.Add(It.IsAny<LeetcodeProblemCategoryEntity>()),
      Times.Exactly(4)
    );
    _dbContextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.AtLeastOnce);
    _dbContextMock.Verify(
      x => x.LeetcodeProblems.Add(It.IsAny<LeetcodeProblemEntity>()),
      Times.Exactly(2)
    );
  }
}
