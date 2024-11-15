using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Graduates;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Graduates;

public class GetGraduatesTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public GetGraduatesTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private GetGraduates.Command ListDummyCommand()
  {
    return new GetGraduates.Command();
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    const string discordId1 = "Discord ID 123";
    const string discordId2 = "Discord ID 456";
    _dbContextMock.Setup(x => x.Users)
                  .ReturnsDbSet(new List<UserEntity>
                  {
                    new()
                    {
                      UserId = DummyId1,
                      DiscordId = discordId1,
                      Email = DummyEmail,
                      Name = DummyName,
                      ProfileImage = DummyProfileImage
                    },
                    new()
                    {
                      UserId = DummyId2,
                      DiscordId = discordId2,
                      Email = DummyEmail,
                      Name = DummyName,
                      ProfileImage = ""
                    }
                  });

    var command = ListDummyCommand();
    var handler =
      new GetGraduates.Handler(_dbContextMock.Object, Mock.Of<ILogger<GetGraduates.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.GraduatesListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.Graduates);

    var graduates = result.ResponseBody.Graduates.ToList();
    Assert.Equal(discordId1, graduates[0].DiscordId);
    Assert.Equal(DummyProfileImage, graduates[0].ProfileImage);
    Assert.Equal(DummyName, graduates[0].Name);
    Assert.Equal(discordId2, graduates[1].DiscordId);
    Assert.Equal("", graduates[1].ProfileImage);
    Assert.Equal(DummyName, graduates[1].Name);
  }
}
