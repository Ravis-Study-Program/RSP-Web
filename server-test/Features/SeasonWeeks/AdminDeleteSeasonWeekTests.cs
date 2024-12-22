using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants; // Added to access Message constants
using RSPWebAPI.Features.SeasonWeeks;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.SeasonWeeks;

public class AdminDeleteSeasonWeekTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminDeleteSeasonWeekTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var existingWeek = new SeasonWeekEntity { SeasonWeekId = DummyId1 };
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { existingWeek });

    var command = new AdminDeleteSeasonWeek.Command { SeasonWeekId = DummyId1 };
    var handler = new AdminDeleteSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminDeleteSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonWeekDeletedSuccessfully, result.SuccessMessage);
  }

  [Fact]
  public async Task Handle_WeekDoesNotExist_BadRequest()
  {
    _dbContextMock.Setup(x => x.SeasonWeeks).ReturnsDbSet(new List<SeasonWeekEntity>());

    var command = new AdminDeleteSeasonWeek.Command { SeasonWeekId = DummyId1 };
    var handler = new AdminDeleteSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminDeleteSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.SeasonWeekDoesNotExists, result.Error?.Message);
  }
}
