using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Seasons;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Seasons;

public class AdminListSeasonTests: TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminListSeasonTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminListSeason.Command ListDummyCommand()
  {
    return new AdminListSeason.Command
    {
    };
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Seasons)
                  .ReturnsDbSet(new List<Season>());

    var command = ListDummyCommand();
    var handler = new AdminListSeason.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminListSeason.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonListSuccessfully, result.SuccessMessage);
  }
}
