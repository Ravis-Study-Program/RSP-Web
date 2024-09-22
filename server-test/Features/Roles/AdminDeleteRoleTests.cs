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

public class AdminDeleteRoleTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminDeleteRoleTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminDeleteRole.Command DeleteDummyCommand()
  {
    return new AdminDeleteRole.Command
    {
      RoleId = DummyGuid
    };
  }

  [Fact]
  public async Task Handle_RoleDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Roles)
                  .ReturnsDbSet(new List<Role>());

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteRole.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteRole.Handler>>());

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

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteRole.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteRole.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.RoleDeletedSuccessfully, result.SuccessMessage);
  }
}