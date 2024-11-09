using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Enrollments;
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
      Email = DummyEmail
    };
  }

  [Fact]
  public async Task Handle_EnrollmentExists_OK()
  {
    var dummyEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Role = SeasonRole.Mentor,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1,
        Name = "Season Name"
      },
      User = new UserEntity
      {
        UserId = DummyId1,
        Email = DummyEmail
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { dummyEnrollment });

    var command = CreateDummyCommand();
    var handler =
      new GetCurrentUserEnrollments.Handler(_dbContextMock.Object,
                                            Mock.Of<ILogger<GetCurrentUserEnrollments.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    var enrollments = result.ResponseBody?.Enrollments.ToList();
    var enrollment = enrollments?[0];
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(SeasonRole.Mentor, enrollment?.Role);
    Assert.Equal("Season Name", enrollment?.SeasonName);
  }
}
