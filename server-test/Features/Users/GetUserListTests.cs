using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Users;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Users;

public class GetUserListTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public GetUserListTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private GetUserList.Command ListDummyCommand()
  {
    return new GetUserList.Command();
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock
      .Setup(x => x.Users)
      .ReturnsDbSet(
        new List<UserEntity>
        {
          new()
          {
            DiscordId = DummyDiscordId,
            Email = DummyEmail,
            UserId = DummyId1,
            IsAdmin = false,
            Name = DummyName,
            ProfileImage = DummyProfileImage,
          },
        }
      );

    var command = ListDummyCommand();
    var handler = new GetUserList.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<GetUserList.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.UserListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.Users);
    Assert.Single(result.ResponseBody.Users);

    var user = result.ResponseBody.Users.ToList();
    Assert.Equal(DummyDiscordId, user[0].DiscordId);
    Assert.Equal(DummyEmail, user[0].Email);
    Assert.Equal(DummyId1, user[0].UserId);
    Assert.False(user[0].IsAdmin);
    Assert.Equal(DummyName, user[0].Name);
    Assert.Equal(DummyProfileImage, user[0].ProfileImage);
  }
}
