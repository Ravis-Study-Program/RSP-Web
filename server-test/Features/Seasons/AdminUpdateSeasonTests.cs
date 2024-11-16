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

public class AdminUpdateSeasonTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminUpdateSeasonTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminUpdateSeason.Command UpdateDummyCommand()
  {
    return new AdminUpdateSeason.Command
    {
      SeasonId = DummyId1,
      Name = DummyName,
      Slug = DummySlug,
      StartDateInclusiveUtc = DummyStartDate,
      EndDateInclusiveUtc = DummyEndDate,
      Location = DummyLocation,
      ImageUrl = DummyImageUrl,
    };
  }

  [Fact]
  public async Task Handle_SeasonDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity>());

    var command = UpdateDummyCommand();
    var handler = new AdminUpdateSeason.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminUpdateSeason.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.SeasonDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var existingSeason = new SeasonEntity { SeasonId = DummyId1 };
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { existingSeason });

    var command = UpdateDummyCommand();
    var handler = new AdminUpdateSeason.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminUpdateSeason.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonUpdatedSuccessfully, result.SuccessMessage);
    Assert.Equal(DummyLocation, result?.ResponseBody?.Location);
  }
}
