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

public class AdminDeleteEnrollmentTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminDeleteEnrollmentTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminDeleteEnrollment.Command DeleteDummyCommand()
  {
    return new AdminDeleteEnrollment.Command
    {
      EnrollmentId = DummyGuid
    };
  }

  [Fact]
  public async Task Handle_EnrollmentDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment>());

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteEnrollment.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteEnrollment.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.EnrollmentDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var existingEnrollment = new Enrollment { EnrollmentId = DummyGuid, SeasonId = DummyGuid, UserId = DummyGuid, RoleId = DummyGuid };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { existingEnrollment });

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteEnrollment.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteEnrollment.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.EnrollmentDeletedSuccessfully, result.SuccessMessage);
  }
}