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

public class AdminDeleteSeasonTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminDeleteSeasonTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminDeleteSeason.Command DeleteDummyCommand()
  {
    return new AdminDeleteSeason.Command
    {
      SeasonId = DummyId1
    };
  }

  [Fact]
  public async Task Handle_SeasonDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Seasons)
                  .ReturnsDbSet(new List<SeasonEntity>());

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteSeason.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteSeason.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.SeasonDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var existingSeason = new SeasonEntity { SeasonId = DummyId1 };
    _dbContextMock.Setup(x => x.Seasons)
                  .ReturnsDbSet(new List<SeasonEntity> { existingSeason });

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteSeason.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteSeason.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonDeletedSuccessfully, result.SuccessMessage);
  }
}