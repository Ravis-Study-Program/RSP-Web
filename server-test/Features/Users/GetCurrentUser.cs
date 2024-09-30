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

public class GetCurrentUserTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public GetCurrentUserTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private GetCurrentUser.Command CreateDummyCommand()
  {
    return new GetCurrentUser.Command
    {
      Email = DummyEmail,
    };
  }

  [Fact]
  public async Task Handle_UserExists_OK()
  {
    var existingUser = new User { Email = DummyEmail, ProfileImage = DummyProfileImage, Name = DummyName };
    _dbContextMock.Setup(x => x.Users)
                  .ReturnsDbSet(new List<User> { existingUser });

    var command = CreateDummyCommand();
    var handler =
      new GetCurrentUser.Handler(_dbContextMock.Object, Mock.Of<ILogger<GetCurrentUser.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(DummyEmail, result.ResponseBody?.User.Email);
    Assert.Equal(DummyProfileImage, result.ResponseBody?.User.ProfileImage);
    Assert.Equal(DummyName, result.ResponseBody?.User.Name);
  }

  [Fact]
  public async Task Handle_UserDoesNotExists_OK()
  {
    _dbContextMock.Setup(x => x.Users)
                  .ReturnsDbSet(new List<User>());

    var command = CreateDummyCommand();
    var handler =
      new GetCurrentUser.Handler(_dbContextMock.Object, Mock.Of<ILogger<GetCurrentUser.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailDoesNotExists, result.Error?.Message);
  }
}