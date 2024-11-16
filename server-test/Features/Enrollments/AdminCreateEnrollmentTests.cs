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

public class AdminCreateEnrollmentTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminCreateEnrollmentTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminCreateEnrollment.Command CreateDummyCommand()
  {
    return new AdminCreateEnrollment.Command
    {
      SeasonId = DummyId1,
      UserId = DummyId1,
      Role = SeasonRole.Coordinator,
      StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
    };
  }

  [Fact]
  public async Task Handle_EntryAlreadyExists_BadRequest()
  {
    var existingEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      SeasonId = DummyId1,
      UserId = DummyId1,
      Role = SeasonRole.Coordinator,
    };
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { existingEnrollment });

    var command = CreateDummyCommand();
    var handler = new AdminCreateEnrollment.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateEnrollment.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.EnrollmentExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_StudentWithInappropriateRolePromotion_BadRequest()
  {
    _dbContextMock.Setup(x => x.Enrollments).ReturnsDbSet(new List<EnrollmentEntity>());

    var command = CreateDummyCommand();
    command.Role = SeasonRole.Student;

    var handler = new AdminCreateEnrollment.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateEnrollment.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.EnrollmentStudentMustHaveAppropriateRolePromotion, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_CoordinatorWithInappropriateRolePromotion_BadRequest()
  {
    _dbContextMock.Setup(x => x.Enrollments).ReturnsDbSet(new List<EnrollmentEntity>());

    var command = CreateDummyCommand();
    command.StudentRolePromotion = SeasonStudentRolePromotion.Beginner;

    var handler = new AdminCreateEnrollment.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateEnrollment.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(
      Message.EnrollmentMentorOrCoordinatorMustNotHaveRolePromotion,
      result?.Error?.Message
    );
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Enrollments).ReturnsDbSet(new List<EnrollmentEntity>());

    var command = CreateDummyCommand();
    var handler = new AdminCreateEnrollment.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminCreateEnrollment.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.EnrollmentCreatedSuccessfully, result.SuccessMessage);
  }
}
