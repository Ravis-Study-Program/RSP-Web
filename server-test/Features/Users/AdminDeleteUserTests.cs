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

public class AdminDeleteUserTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminDeleteUserTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminDeleteUser.Command DeleteDummyCommand()
  {
    return new AdminDeleteUser.Command
    {
      Email = DummyEmail
    };
  }

  [Fact]
  public async Task Handle_UserDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Users)
                  .ReturnsDbSet(new List<User>());

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteUser.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteUser.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UserEmailDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var existingUser = new User { Email = DummyEmail };
    _dbContextMock.Setup(x => x.Users)
                  .ReturnsDbSet(new List<User> { existingUser });

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteUser.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteUser.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.UserDeletedSuccessfully, result.SuccessMessage);
  }
}