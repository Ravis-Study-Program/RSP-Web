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

public class AdminCreateUserTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminCreateUserTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminCreateUser.Command CreateDummyCommand()
  {
    return new AdminCreateUser.Command
    {
      DiscordId = DummyDiscordId,
      Email = DummyEmail,
      ProfileImage = DummyProfileImage,
      Name = DummyName,
    };
  }

  [Fact]
  public async Task Handle_UserAlreadyExists_BadRequest()
  {
    var existingUser = new UserEntity { Email = DummyEmail };
    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity> { existingUser });

    var command = CreateDummyCommand();
    var handler = new AdminCreateUser.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateUser.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Users).ReturnsDbSet(new List<UserEntity>());

    var command = CreateDummyCommand();
    var handler = new AdminCreateUser.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateUser.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.UserCreatedSuccessfully, result.SuccessMessage);
  }
}
