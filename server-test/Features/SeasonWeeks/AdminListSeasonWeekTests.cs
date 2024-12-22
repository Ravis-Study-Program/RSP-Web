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

public class AdminListSeasonWeekTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminListSeasonWeekTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var weeks = new List<SeasonWeekEntity>
    {
      new() { SeasonWeekId = DummyId1, SeasonId = DummyId2 },
      new() { SeasonWeekId = DummyId3, SeasonId = DummyId1 },
    };
    _dbContextMock.Setup(x => x.SeasonWeeks).ReturnsDbSet(weeks);

    var command = new AdminListSeasonWeek.Command();
    var handler = new AdminListSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminListSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonWeekListSuccessfully, result.SuccessMessage);
    Assert.Equal(2, result.ResponseBody?.SeasonWeeks.Count);
  }
}
