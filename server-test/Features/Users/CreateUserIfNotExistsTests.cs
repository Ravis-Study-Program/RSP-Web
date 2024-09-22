using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Users;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Users;

public class CreateUserIfNotExistsTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public CreateUserIfNotExistsTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private CreateUserIfNotExists.Command CreateDummyCommand()
  {
    return new CreateUserIfNotExists.Command
    {
      DiscordId = DummyDiscordId,
      Email = DummyEmail,
      ProfileImage = DummyProfileImage,
      Name = DummyName
    };
  }

  [Fact]
  public async Task Handle_UserExists_OK()
  {
    var existingUser = new User { Email = DummyEmail };
    _dbContextMock.Setup(x => x.Users)
                  .ReturnsDbSet(new List<User> { existingUser });

    var command = CreateDummyCommand();
    var handler =
      new CreateUserIfNotExists.Handler(_dbContextMock.Object, Mock.Of<ILogger<CreateUserIfNotExists.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
  }

  [Fact]
  public async Task Handle_UserDoesNotExists_OK()
  {
    _dbContextMock.Setup(x => x.Users)
                  .ReturnsDbSet(new List<User>());

    var command = CreateDummyCommand();
    var handler =
      new CreateUserIfNotExists.Handler(_dbContextMock.Object, Mock.Of<ILogger<CreateUserIfNotExists.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
  }
}