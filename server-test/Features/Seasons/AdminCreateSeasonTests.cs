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

public class AdminCreateSeasonTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminCreateSeasonTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminCreateSeason.Command CreateDummyCommand()
  {
    return new AdminCreateSeason.Command
    {
      Name = DummyName,
      Slug = DummySlug,
      StartDateInclusiveUtc = DummyStartDate,
      EndDateInclusiveUtc = DummyEndDate,
      Location = DummyLocation,
      ImageUrl = DummyImageUrl,
    };
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Seasons).ReturnsDbSet(new List<SeasonEntity>());

    var command = CreateDummyCommand();
    var handler = new AdminCreateSeason.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateSeason.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonCreatedSuccessfully, result.SuccessMessage);
  }
}
