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

public class GetIsUserEnrolledTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public GetIsUserEnrolledTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private GetIsUserEnrolled.Command CreateDummyCommand()
  {
    return new GetIsUserEnrolled.Command
    {
      Email = DummyEmail,
      SeasonSlug = "Season Slug"
    };
  }

  [Fact]
  public async Task Handle_UserEnrollmentExists_OK()
  {
    var dummyEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1,
        Name = "Season Name",
        Slug = "Season Slug"
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
      new GetIsUserEnrolled.Handler(_dbContextMock.Object, Mock.Of<ILogger<GetIsUserEnrolled.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    var response = result.ResponseBody;
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(true, response?.IsEnrolled);
  }

  [Fact]
  public async Task Handle_UserEnrollmentDoesNotExists_OK()
  {
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity>());

    var command = CreateDummyCommand();
    var handler =
      new GetIsUserEnrolled.Handler(_dbContextMock.Object, Mock.Of<ILogger<GetIsUserEnrolled.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    var response = result.ResponseBody;
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(false, response?.IsEnrolled);
  }
}
