using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Mentorships;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.Mentorships;

public class AdminUpdateMentorshipTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminUpdateMentorshipTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminUpdateMentorship.Command UpdateDummyCommand()
  {
    return new AdminUpdateMentorship.Command
    {
      MentorshipId = DummyId1,
      MentorEnrollmentId = DummyId1,
      MenteeEnrollmentId = DummyId2
    };
  }

  [Fact]
  public async Task Handle_SeasonDoesNotMatch_BadRequest()
  {
    var mentorEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Role = SeasonRole.Mentor,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1
      }
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Role = SeasonRole.Student,
      Season = new SeasonEntity
      {
        SeasonId = DummyId2
      }
    };
    var existingMentorship = new MentorshipEntity
    {
      MentorshipId = DummyId1,
      MentorEnrollmentId = DummyId1,
      MentorEnrollment = mentorEnrollment,
      MenteeEnrollmentId = DummyId2,
      MenteeEnrollment = menteeEnrollment
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { menteeEnrollment, mentorEnrollment });
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<MentorshipEntity> { existingMentorship });

    var command = UpdateDummyCommand();
    var handler =
      new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipNotPermittedDueToDifferentSeason, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_MentorDoesNotExist_BadRequest()
  {
    var mentorEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Role = SeasonRole.Mentor,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1
      }
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Role = SeasonRole.Student,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1
      }
    };
    var existingMentorship = new MentorshipEntity
    {
      MentorshipId = DummyId1,
      MentorEnrollmentId = DummyId1,
      MentorEnrollment = mentorEnrollment,
      MenteeEnrollmentId = DummyId2,
      MenteeEnrollment = menteeEnrollment
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { menteeEnrollment, mentorEnrollment });
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<MentorshipEntity> { existingMentorship });

    var command = new AdminUpdateMentorship.Command
    {
      MentorshipId = DummyId1,
      MentorEnrollmentId = DummyId3,
      MenteeEnrollmentId = DummyId1
    };
    var handler =
      new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipNotPermittedDueToNullMentorOrMentee, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_MenteeDoesNotExist_BadRequest()
  {
    var mentorEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Role = SeasonRole.Mentor,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1
      }
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Role = SeasonRole.Student,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1
      }
    };
    var existingMentorship = new MentorshipEntity
    {
      MentorshipId = DummyId1,
      MentorEnrollmentId = DummyId1,
      MentorEnrollment = mentorEnrollment,
      MenteeEnrollmentId = DummyId2,
      MenteeEnrollment = menteeEnrollment
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { menteeEnrollment, mentorEnrollment });
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<MentorshipEntity> { existingMentorship });

    var command = new AdminUpdateMentorship.Command
    {
      MentorshipId = DummyId1,
      MentorEnrollmentId = DummyId1,
      MenteeEnrollmentId = DummyId3
    };
    var handler =
      new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipNotPermittedDueToNullMentorOrMentee, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_MentorshipDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<MentorshipEntity>());

    var command = UpdateDummyCommand();
    var handler =
      new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipDoesNotExists, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var targetEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId3,
      Role = SeasonRole.Student,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1
      }
    };
    var mentorEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Role = SeasonRole.Mentor,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1
      }
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Role = SeasonRole.Student,
      Season = new SeasonEntity
      {
        SeasonId = DummyId1
      }
    };
    var existingMentorship = new MentorshipEntity
    {
      MentorshipId = DummyId1,
      MentorEnrollmentId = DummyId1,
      MentorEnrollment = mentorEnrollment,
      MenteeEnrollmentId = DummyId2,
      MenteeEnrollment = menteeEnrollment
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<EnrollmentEntity> { menteeEnrollment, mentorEnrollment, targetEnrollment });
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<MentorshipEntity> { existingMentorship });

    var command = new AdminUpdateMentorship.Command
    {
      MentorshipId = DummyId1,
      MentorEnrollmentId = DummyId1,
      MenteeEnrollmentId = DummyId3
    };
    var handler =
      new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MentorshipUpdatedSuccessfully, result.SuccessMessage);
  }
}
