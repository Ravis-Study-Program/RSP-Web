using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments;
using RSPWebAPI.Features.Users;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Enrollments;

public class GetCurrentUserEnrollmentsTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public GetCurrentUserEnrollmentsTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private GetCurrentUserEnrollments.Command CreateDummyCommand()
  {
    return new GetCurrentUserEnrollments.Command
    {
      Email = DummyEmail,
    };
  }

  [Fact]
  public async Task Handle_EnrollmentExists_OK()
  {
    var dummyEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid,
      Season = new Season
      {
        SeasonId = DummyGuid,
        Name = "Season Name"
      },
      Role = new Role
      {
        RoleId = DummyGuid,
        Name = "Role Name"
      },
      User = new User
      {
        UserId = DummyGuid,
        Email = DummyEmail
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { dummyEnrollment });

    var command = CreateDummyCommand();
    var handler =
      new GetCurrentUserEnrollments.Handler(_dbContextMock.Object, Mock.Of<ILogger<GetCurrentUserEnrollments.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    var enrollments = result.ResponseBody?.Enrollments.ToList();
    var enrollment = enrollments?[0];
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(DummyGuid, enrollment?.EnrollmentId);
    Assert.Equal("Season Name", enrollment?.Season.Name);
    Assert.Equal("Role Name", enrollment?.Role.Name);
  }
}