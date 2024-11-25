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

public class AdminListSeasonTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminListSeasonTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminListSeason.Command ListDummyCommand()
  {
    return new AdminListSeason.Command();
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock
      .Setup(x => x.Seasons)
      .ReturnsDbSet(
        new List<SeasonEntity>
        {
          new()
          {
            SeasonId = DummyId1,
            Slug = DummySlug,
            Name = DummyName,
            Location = DummyLocation,
            StartDateInclusiveUtc = DummyStartDate,
            EndDateInclusiveUtc = DummyEndDate,
          },
        }
      );

    var command = ListDummyCommand();
    var handler = new AdminListSeason.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminListSeason.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.Seasons);
    Assert.Single(result.ResponseBody.Seasons);

    var season = result.ResponseBody.Seasons.ToList();
    Assert.Equal(DummyId1, season[0].SeasonId);
    Assert.Equal(DummySlug, season[0].Slug);
    Assert.Equal(DummyName, season[0].Name);
    Assert.Equal(DummyLocation, season[0].Location);
    Assert.Equal(DummyStartDate, season[0].StartDateInclusiveUtc);
    Assert.Equal(DummyEndDate, season[0].EndDateInclusiveUtc);
  }
}
