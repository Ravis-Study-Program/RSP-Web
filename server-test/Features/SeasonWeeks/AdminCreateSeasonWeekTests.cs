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

public class AdminCreateSeasonWeekTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminCreateSeasonWeekTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminCreateSeasonWeek.Command CreateDummyCommand()
  {
    return new AdminCreateSeasonWeek.Command
    {
      SeasonId = DummyId1,
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
      SeasonId = DummyId1,
      StartDateInclusiveUtc = DummyStartDate.AddDays(-1),
      EndDateInclusiveUtc = DummyEndDate.AddDays(1),
    };
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock.Setup(x => x.SeasonWeeks).ReturnsDbSet(new List<SeasonWeekEntity>());

    var command = CreateDummyCommand();
    var handler = new AdminCreateSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonWeekCreatedSuccessfully, result.SuccessMessage);
  }

  [Fact]
  public async Task Handle_SeasonDoesNotExist_BadRequest()
  {
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity>());

    var command = CreateDummyCommand();
    var handler = new AdminCreateSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.SeasonDoesNotExists, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_WeekNumberExists_BadRequest()
  {
    var season = new SeasonEntity
    {
      SeasonId = DummyId1,
      StartDateInclusiveUtc = DummyStartDate.AddDays(-1),
      EndDateInclusiveUtc = DummyEndDate.AddDays(1),
    };
    var existingWeek = new SeasonWeekEntity { SeasonId = DummyId1, WeekNumber = 1 };

    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock
      .Setup(x => x.SeasonWeeks)
      .ReturnsDbSet(new List<SeasonWeekEntity> { existingWeek });

    var command = CreateDummyCommand();
    var handler = new AdminCreateSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateSeasonWeek.Handler>>()
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
      SeasonId = DummyId1,
      StartDateInclusiveUtc = DummyStartDate.AddDays(1),
      EndDateInclusiveUtc = DummyEndDate.AddDays(-1),
    };

    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity> { season });
    _dbContextMock.Setup(x => x.SeasonWeeks).ReturnsDbSet(new List<SeasonWeekEntity>());

    var command = CreateDummyCommand();
    var handler = new AdminCreateSeasonWeek.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateSeasonWeek.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.SeasonWeekDatesNotWithinSeasonDates, result.Error?.Message);
  }
}
