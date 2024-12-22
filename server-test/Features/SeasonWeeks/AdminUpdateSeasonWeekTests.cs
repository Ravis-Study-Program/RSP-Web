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

public class AdminUpdateSeasonWeekTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminUpdateSeasonWeekTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminUpdateSeasonWeek.Command CreateDummyCommand()
  {
    return new AdminUpdateSeasonWeek.Command
    {
      SeasonWeekId = DummyId1,
      WeekNumber = 1,
      StartDate = DummyStartDate,
      EndDate = DummyEndDate,
    };
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var season = new SeasonEntity
    {
      SeasonId = DummyId2,
      StartDateInclusiveUtc = DummyStartDate.AddDays(-1),
      EndDateInclusiveUtc = DummyEndDate.AddDays(1),
    };
    var existingWeek = new SeasonWeekEntity
    {
      SeasonWeekId = DummyId1,
      SeasonId = DummyId2,
      Season = season,
    };

    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { existingWeek });

    var command = CreateDummyCommand();
    var handler = new AdminUpdateSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminUpdateSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonWeekUpdateSuccessfully, result.SuccessMessage);
  }

  [Fact]
  public async Task Handle_WeekDoesNotExist_BadRequest()
  {
    _dbContextMock.Setup(x => x.SeasonWeeks).ReturnsDbSet(new List<SeasonWeekEntity>());

    var command = CreateDummyCommand();
    var handler = new AdminUpdateSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminUpdateSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.SeasonWeekDoesNotExists, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_WeekNumberConflict_BadRequest()
  {
    var season = new SeasonEntity
    {
      SeasonId = DummyId2,
      StartDateInclusiveUtc = DummyStartDate.AddDays(-1),
      EndDateInclusiveUtc = DummyEndDate.AddDays(1),
    };
    var existingWeek = new SeasonWeekEntity
    {
      SeasonWeekId = DummyId1,
      SeasonId = DummyId2,
      Season = season,
    };
    var conflictingWeek = new SeasonWeekEntity
    {
      SeasonWeekId = DummyId3,
      SeasonId = DummyId2,
      WeekNumber = 1,
    };

    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { existingWeek, conflictingWeek });

    var command = CreateDummyCommand();
    var handler = new AdminUpdateSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminUpdateSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.SeasonWeekWeekNumberAlreadyExists, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_DatesOutsideSeasonRange_BadRequest()
  {
    var season = new SeasonEntity
    {
      SeasonId = DummyId2,
      StartDateInclusiveUtc = DummyStartDate.AddDays(1),
      EndDateInclusiveUtc = DummyEndDate.AddDays(-1),
    };
    var existingWeek = new SeasonWeekEntity
    {
      SeasonWeekId = DummyId1,
      SeasonId = DummyId2,
      Season = season,
    };

    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { existingWeek });

    var command = CreateDummyCommand();
    var handler = new AdminUpdateSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminUpdateSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.SeasonWeekDatesNotWithinSeasonDates, result.Error?.Message);
  }
}
