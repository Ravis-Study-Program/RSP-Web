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

public class AdminUpdateUserTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminUpdateUserTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminUpdateUser.Command UpdateDummyCommand()
  {
    return new AdminUpdateUser.Command
    {
      DiscordId = DummyDiscordId,
      Email = DummyEmail,
      ProfileImage = DummyProfileImage,
      Name = DummyName,
    };
  }

  [Fact]
  public async Task Handle_UserDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity>());

    var command = UpdateDummyCommand();
    var handler = new AdminUpdateUser.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminUpdateUser.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var existingUser = new UserEntity { Email = DummyEmail };
    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { existingUser });

    var command = UpdateDummyCommand();
    var handler = new AdminUpdateUser.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminUpdateUser.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.UserUpdatedSuccessfully, result.SuccessMessage);
    Assert.Equal(DummyDiscordId, result?.ResponseBody?.DiscordId);
  }
}
