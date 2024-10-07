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
      MentorshipId = DummyGuid,
      MentorEnrollmentId = DummyGuid,
      MenteeEnrollmentId = DummyGuid2
    };
  }
  
  [Fact]
  public async Task Handle_SeasonDoesNotMatch_BadRequest()
  {
    var mentorEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid,
      Season = new Season
      {
        SeasonId = DummyGuid
      }
    };
    var menteeEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid2,
      Season = new Season
      {
        SeasonId = DummyGuid2
      }
    };
    var existingMentorship = new Mentorship
    {
      MentorshipId = DummyGuid,
      MentorEnrollmentId = DummyGuid,
      MentorEnrollment = mentorEnrollment,
      MenteeEnrollmentId = DummyGuid2,
      MenteeEnrollment = menteeEnrollment
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { menteeEnrollment, mentorEnrollment });
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<Mentorship> { existingMentorship });

    var command = UpdateDummyCommand();
    var handler = new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipNotPermittedDueToDifferentSeason, result?.Error?.Message);
  }
  
  [Fact]
  public async Task Handle_MentorDoesNotExist_BadRequest()
  {
    var mentorEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid,
      Season = new Season
      {
        SeasonId = DummyGuid
      }
    };
    var menteeEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid2,
      Season = new Season
      {
        SeasonId = DummyGuid
      }
    };
    var existingMentorship = new Mentorship
    {
      MentorshipId = DummyGuid,
      MentorEnrollmentId = DummyGuid,
      MentorEnrollment = mentorEnrollment,
      MenteeEnrollmentId = DummyGuid2,
      MenteeEnrollment = menteeEnrollment
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { menteeEnrollment, mentorEnrollment });
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<Mentorship> { existingMentorship });

    var command = new AdminUpdateMentorship.Command
    {
      MentorshipId = DummyGuid,
      MentorEnrollmentId = DummyGuid3,
      MenteeEnrollmentId = DummyGuid
    };
    var handler = new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipNotPermittedDueToNullMentorOrMentee, result.Error?.Message);
  }
  
  [Fact]
  public async Task Handle_MenteeDoesNotExist_BadRequest()
  {
    var mentorEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid,
      Season = new Season
      {
        SeasonId = DummyGuid
      }
    };
    var menteeEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid2,
      Season = new Season
      {
        SeasonId = DummyGuid
      }
    };
    var existingMentorship = new Mentorship
    {
      MentorshipId = DummyGuid,
      MentorEnrollmentId = DummyGuid,
      MentorEnrollment = mentorEnrollment,
      MenteeEnrollmentId = DummyGuid2,
      MenteeEnrollment = menteeEnrollment
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { menteeEnrollment, mentorEnrollment });
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<Mentorship> { existingMentorship });

    var command = new AdminUpdateMentorship.Command
    {
      MentorshipId = DummyGuid,
      MentorEnrollmentId = DummyGuid,
      MenteeEnrollmentId = DummyGuid3
    };
    var handler = new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipNotPermittedDueToNullMentorOrMentee, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_MentorshipDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<Mentorship>());

    var command = UpdateDummyCommand();
    var handler = new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipDoesNotExists, result.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var targetEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid3,
      Season = new Season
      {
        SeasonId = DummyGuid
      }
    };
    var mentorEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid,
      Season = new Season
      {
        SeasonId = DummyGuid
      }
    };
    var menteeEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid2,
      Season = new Season
      {
        SeasonId = DummyGuid
      }
    };
    var existingMentorship = new Mentorship
    {
      MentorshipId = DummyGuid,
      MentorEnrollmentId = DummyGuid,
      MentorEnrollment = mentorEnrollment,
      MenteeEnrollmentId = DummyGuid2,
      MenteeEnrollment = menteeEnrollment
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { menteeEnrollment, mentorEnrollment, targetEnrollment });
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<Mentorship> { existingMentorship });

    var command = new AdminUpdateMentorship.Command
    {
      MentorshipId = DummyGuid,
      MentorEnrollmentId = DummyGuid,
      MenteeEnrollmentId = DummyGuid3
    };
    var handler = new AdminUpdateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminUpdateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MentorshipUpdatedSuccessfully, result.SuccessMessage);
  }
}
