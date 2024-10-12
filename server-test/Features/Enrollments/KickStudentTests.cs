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
      MenteeEnrollmentId = DummyGuid,
      Email = DummyEmail,
      SeasonSlug = DummySlug
    };
  }

  [Fact]
  public async Task Handle_CurrentUserEnrollmentDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment>());

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.KickStudentCurrentUserEnrollmentDoesNotExists, result?.Error?.Message);
  }
  
  [Fact]
  public async Task Handle_CurrentUserIsNotMentorOrCoordinator_BadRequest()
  {
    var currentUserEnrollmentStudent = new Enrollment
    {
      EnrollmentId = DummyGuid2, 
      Season = new Season
      {
        Slug = DummySlug
      }, 
      Role = new Role
      {
        Name = "Student"
      },
      User = new User
      {
        Email = DummyEmail
      }
    };
    var menteeEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid, 
      Season = new Season
      {
        Slug = DummySlug
      }, 
      Role = new Role
      {
        Name = "Student"
      },
      User = new User
      {
        Email = "mentee@email.com"
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { currentUserEnrollmentStudent, menteeEnrollment });

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.KickStudentCurrentUserEnrollmentDoesNotExists, result?.Error?.Message);
  }

  
  [Fact]
  public async Task Handle_MenteeIsNotStudent_BadRequest()
  {
    var currentUserEnrollmentCoordinator = new Enrollment
    {
      EnrollmentId = DummyGuid2, 
      Season = new Season
      {
        Slug = DummySlug
      }, 
      Role = new Role
      {
        Name = "Coordinator"
      },
      User = new User
      {
        Email = DummyEmail
      }
    };
    var menteeEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid, 
      Season = new Season
      {
        Slug = DummySlug
      }, 
      Role = new Role
      {
        Name = "Mentor"
      },
      User = new User
      {
        Email = "mentee@email.com"
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { currentUserEnrollmentCoordinator, menteeEnrollment });

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.KickStudentMenteeDoesntExist, result?.Error?.Message);
  }

  
  [Fact]
  public async Task Handle_Success_Coordinator_OK()
  {
    var currentUserEnrollmentCoordinator = new Enrollment
    {
      EnrollmentId = DummyGuid2, 
      Season = new Season
      {
        Slug = DummySlug
      }, 
      Role = new Role
      {
        Name = "Coordinator"
      },
      User = new User
      {
        Email = DummyEmail
      }
    };
    var menteeEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid, 
      Season = new Season
      {
        Slug = DummySlug
      }, 
      Role = new Role
      {
        Name = "Student"
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { currentUserEnrollmentCoordinator, menteeEnrollment });

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.KickStudentSuccessfully, result.SuccessMessage);
  }

  [Fact]
  public async Task Handle_Success_Mentor_OK()
  {
    var currentUserEnrollmentMentor = new Enrollment
    {
      EnrollmentId = DummyGuid2, 
      Season = new Season
      {
        Slug = DummySlug
      }, 
      Role = new Role
      {
        Name = "Mentor"
      },
      User = new User
      {
        Email = DummyEmail
      }
    };
    var menteeEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid, 
      Season = new Season
      {
        Slug = DummySlug
      }, 
      Role = new Role
      {
        Name = "Student"
      },
      User = new User
      {
        Email = "mentee@email.com"
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { currentUserEnrollmentMentor, menteeEnrollment });

    var command = DeleteDummyCommand();
    var handler = new KickStudent.Handler(_dbContextMock.Object, Mock.Of<ILogger<KickStudent.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.KickStudentSuccessfully, result.SuccessMessage);
  }
}
