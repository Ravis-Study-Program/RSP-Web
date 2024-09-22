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

public class AdminCreateRoleTests: TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminCreateRoleTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminCreateRole.Command CreateDummyCommand()
  {
    return new AdminCreateRole.Command
    {
      Name = DummyName,
      StartDate = DummyStartDate,
      EndDate = DummyEndDate,
      Location = DummyLocation
    };
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Roles)
                  .ReturnsDbSet(new List<Role>());

    var command = CreateDummyCommand();
    var handler = new AdminCreateRole.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminCreateRole.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.RoleCreatedSuccessfully, result.SuccessMessage);
  }
}