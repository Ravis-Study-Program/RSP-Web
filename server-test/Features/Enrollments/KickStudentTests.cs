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

public class KickStudentTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public KickStudentTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private KickStudent.Command DeleteDummyCommand()
  {
    return new KickStudent.Command
    {
      MenteeEnrollmentId = DummyId1,
      Email = DummyEmail,
      SeasonSlug = DummySlug
    };
  }

  [Fact]
  public async Task Handle_CurrentUserEnrollmentDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity>());

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.KickStudentCurrentUserEnrollmentDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_CurrentUserIsNotMentorOrCoordinator_BadRequest()
  {
    var currentUserEnrollmentStudent = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Season = new SeasonEntity
      {
        Slug = DummySlug
      },
      Role = SeasonRole.Student,
      User = new UserEntity
      {
        Email = DummyEmail
      }
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Season = new SeasonEntity
      {
        Slug = DummySlug
      },
      User = new UserEntity
      {
        Email = "mentee@email.com"
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { currentUserEnrollmentStudent, menteeEnrollment });

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.KickStudentCurrentUserEnrollmentDoesNotExists, result?.Error?.Message);
  }


  [Fact]
  public async Task Handle_MenteeIsNotStudent_BadRequest()
  {
    var currentUserEnrollmentCoordinator = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Season = new SeasonEntity
      {
        Slug = DummySlug
      },
      Role = SeasonRole.Coordinator,
      User = new UserEntity
      {
        Email = DummyEmail
      }
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Season = new SeasonEntity
      {
        Slug = DummySlug
      },
      Role = SeasonRole.Mentor,
      User = new UserEntity
      {
        Email = "mentee@email.com"
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { currentUserEnrollmentCoordinator, menteeEnrollment });

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.KickStudentMenteeDoesntExist, result?.Error?.Message);
  }


  [Fact]
  public async Task Handle_Success_Coordinator_OK()
  {
    var currentUserEnrollmentCoordinator = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Season = new SeasonEntity
      {
        Slug = DummySlug
      },
      Role = SeasonRole.Coordinator,
      User = new UserEntity
      {
        Email = DummyEmail
      }
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Role = SeasonRole.Student,
      Season = new SeasonEntity
      {
        Slug = DummySlug
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { currentUserEnrollmentCoordinator, menteeEnrollment });

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.KickStudentSuccessfully, result.SuccessMessage);
  }

  [Fact]
  public async Task Handle_Success_Mentor_OK()
  {
    var currentUserEnrollmentMentor = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Season = new SeasonEntity
      {
        Slug = DummySlug
      },
      Role = SeasonRole.Mentor,
      User = new UserEntity
      {
        Email = DummyEmail
      }
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Season = new SeasonEntity
      {
        Slug = DummySlug
      },
      Role = SeasonRole.Student,
      User = new UserEntity
      {
        Email = "mentee@email.com"
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { currentUserEnrollmentMentor, menteeEnrollment });

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.KickStudentSuccessfully, result.SuccessMessage);
  }
}
