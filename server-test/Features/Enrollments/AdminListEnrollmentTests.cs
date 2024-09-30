using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Enrollments;

public class AdminListEnrollmentTests: TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminListEnrollmentTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminListEnrollment.Command ListDummyCommand()
  {
    return new AdminListEnrollment.Command
    {
    };
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment>()
                  {
                    new Enrollment()
                    {
                      EnrollmentId = DummyGuid,
                      SeasonId = DummyGuid,
                      UserId = DummyGuid,
                      RoleId = DummyGuid,
                      Season = new Season()
                      {
                        SeasonId = DummyGuid,
                        Name = "Season Name",
                        Location = DummyLocation
                      },
                      User = new User()
                      {
                        Name = "User Name",
                        Email = DummyEmail
                      },
                      Role = new Role()
                      {
                        RoleId = DummyGuid,
                        Name = "Role Name"
                      }
                    }
                  });

    var command = ListDummyCommand();
    var handler = new AdminListEnrollment.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminListEnrollment.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.EnrollmentListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.Enrollments);
    Assert.Single(result.ResponseBody.Enrollments);

    var enrollments = result.ResponseBody.Enrollments.ToList();
    var enrollment = enrollments[0];
    Assert.Equal(DummyGuid, enrollment.EnrollmentId);
    Assert.Equal(DummyGuid, enrollment.RoleId);
    Assert.Equal(DummyGuid, enrollment.UserId);
    Assert.Equal(DummyGuid, enrollment.SeasonId);
    Assert.Equal("Role Name", enrollment.Role);
    Assert.Equal("User Name", enrollment.User);
    Assert.Equal("Season Name", enrollment.Season);
  }
}
