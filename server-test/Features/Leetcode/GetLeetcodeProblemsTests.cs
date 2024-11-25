using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Leetcode;
using Xunit;

namespace RSPWebAPI.Tests.Features.Leetcode;

public class GetLeetcodeProblemsTests
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;
  private readonly Mock<ILogger<GetLeetcodeProblems.Handler>> _loggerMock;

  public GetLeetcodeProblemsTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
    _loggerMock = new Mock<ILogger<GetLeetcodeProblems.Handler>>();
  }

  private GetLeetcodeProblems.Command DummyCommand() => new();

  [Fact]
  public async Task Handle_Success_OK()
  {
    // Arrange
    _dbContextMock
      .Setup(x => x.LeetcodeProblems)
      .ReturnsDbSet(
        new List<LeetcodeProblemEntity>
        {
          new()
          {
            LeetcodeProblemId = "1",
            IsPremium = true,
            Problem = new ProblemEntity
            {
              Title = "Two Sum",
              Link = "https://leetcode.com/problems/two-sum/",
            },
            LeetcodeProblemDifficulty = LeetcodeProblemDifficulty.Easy,
            LeetcodeProblemCategories = new List<LeetcodeProblemCategoryEntity>
            {
              new() { Name = "Array" },
              new() { Name = "Hash Table" },
            },
          },
          new()
          {
            LeetcodeProblemId = "2",
            IsPremium = false,
            Problem = new ProblemEntity
            {
              Title = "Add Two Numbers",
              Link = "https://leetcode.com/problems/add-two-numbers/",
            },
            LeetcodeProblemDifficulty = LeetcodeProblemDifficulty.Medium,
            LeetcodeProblemCategories = new List<LeetcodeProblemCategoryEntity>
            {
              new() { Name = "Linked List" },
              new() { Name = "Math" },
            },
          },
        }
      );

    var command = DummyCommand();
    var handler = new GetLeetcodeProblems.Handler(_dbContextMock.Object, _loggerMock.Object);

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.LeetcodeProblemsListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.LeetcodeProblems);
    Assert.Equal(2, result.ResponseBody.LeetcodeProblems.Count);

    var problems = result.ResponseBody.LeetcodeProblems.ToList();

    var problem1 = problems[0];
    Assert.Equal("1", problem1.LeetcodeProblemId);
    Assert.True(problem1.IsPremium);
    Assert.Equal("Two Sum", problem1.Title);
    Assert.Equal("https://leetcode.com/problems/two-sum/", problem1.Link);
    Assert.Equal(LeetcodeProblemDifficulty.Easy, problem1.Difficulty);
    Assert.Equal(2, problem1.LeetcodeProblemCategories.Count);
    Assert.Contains(problem1.LeetcodeProblemCategories, c => c.Name == "Array");
    Assert.Contains(problem1.LeetcodeProblemCategories, c => c.Name == "Hash Table");

    var problem2 = problems[1];
    Assert.Equal("2", problem2.LeetcodeProblemId);
    Assert.False(problem2.IsPremium);
    Assert.Equal("Add Two Numbers", problem2.Title);
    Assert.Equal("https://leetcode.com/problems/add-two-numbers/", problem2.Link);
    Assert.Equal(LeetcodeProblemDifficulty.Medium, problem2.Difficulty);
    Assert.Equal(2, problem2.LeetcodeProblemCategories.Count);
    Assert.Contains(problem2.LeetcodeProblemCategories, c => c.Name == "Linked List");
    Assert.Contains(problem2.LeetcodeProblemCategories, c => c.Name == "Math");
  }
}
