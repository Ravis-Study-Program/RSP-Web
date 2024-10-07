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

public class AdminUpdateRoleTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminUpdateRoleTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminUpdateRole.Command UpdateDummyCommand()
  {
    return new AdminUpdateRole.Command
    {
      RoleId = DummyGuid,
      Name = DummyName,
    };
  }

  [Fact]
  public async Task Handle_RoleDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Roles)
                  .ReturnsDbSet(new List<Role>());

    var command = UpdateDummyCommand();
    var handler = new AdminUpdateRole.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateRole.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.RoleDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var existingRole = new Role { RoleId = DummyGuid };
    _dbContextMock.Setup(x => x.Roles)
                  .ReturnsDbSet(new List<Role> { existingRole });

    var command = UpdateDummyCommand();
    var handler = new AdminUpdateRole.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateRole.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.RoleUpdatedSuccessfully, result.SuccessMessage);
    Assert.Equal(DummyName, result?.ResponseBody?.Name);
  }
}