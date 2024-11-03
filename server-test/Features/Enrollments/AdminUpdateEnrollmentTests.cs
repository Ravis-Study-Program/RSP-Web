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

public class AdminUpdateEnrollmentTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminUpdateEnrollmentTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminUpdateEnrollment.Command UpdateDummyCommand()
  {
    return new AdminUpdateEnrollment.Command
    {
      EnrollmentId = DummyId1,
      SeasonId = DummyId1,
      UserId = DummyId1
    };
  }

  [Fact]
  public async Task Handle_EnrollmentDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity>());

    var command = UpdateDummyCommand();
    var handler =
      new AdminUpdateEnrollment.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateEnrollment.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.EnrollmentDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var existingEnrollment = new EnrollmentEntity { EnrollmentId = DummyId1 };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { existingEnrollment });

    var command = UpdateDummyCommand();
    var handler =
      new AdminUpdateEnrollment.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateEnrollment.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.EnrollmentUpdatedSuccessfully, result.SuccessMessage);
    Assert.Equal(DummyId1, result?.ResponseBody?.UserId);
  }
}
