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

public class AdminListUserTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminListUserTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminListUser.Command ListDummyCommand()
  {
    return new AdminListUser.Command();
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Users)
                  .ReturnsDbSet(new List<UserEntity>());

    var command = ListDummyCommand();
    var handler = new AdminListUser.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminListUser.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.UserListSuccessfully, result.SuccessMessage);
  }
}
