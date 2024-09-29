using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Roles;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Roles;

public class AdminListRoleTests: TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminListRoleTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminListRole.Command ListDummyCommand()
  {
    return new AdminListRole.Command
    {
    };
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Roles)
                  .ReturnsDbSet(new List<Role>());

    var command = ListDummyCommand();
    var handler = new AdminListRole.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminListRole.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.RoleListSuccessfully, result.SuccessMessage);
  }
}
