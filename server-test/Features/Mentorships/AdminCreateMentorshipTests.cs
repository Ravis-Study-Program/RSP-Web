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

public class AdminCreateMentorshipTests: TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminCreateMentorshipTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminCreateMentorship.Command CreateDummyCommand()
  {
    return new AdminCreateMentorship.Command
    {
      MentorEnrollmentId = DummyGuid,
      MenteeEnrollmentId = DummyGuid2
    };
  }
  
  [Fact]
  public async Task Handle_EntryAlreadyExists_BadRequest()
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

    var command = CreateDummyCommand();
    var handler = new AdminCreateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminCreateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipExists, result?.Error?.Message);
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
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { menteeEnrollment, mentorEnrollment });

    var command = CreateDummyCommand();
    var handler = new AdminCreateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminCreateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipNotPermittedDueToDifferentSeason, result?.Error?.Message);
  }
  
  [Fact]
  public async Task Handle_MentorDoesNotExist_BadRequest()
  {
    var menteeEnrollment = new Enrollment
    {
      EnrollmentId = DummyGuid2,
      Season = new Season
      {
        SeasonId = DummyGuid2
      }
    };
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { menteeEnrollment });

    var command = CreateDummyCommand();
    var handler = new AdminCreateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminCreateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipNotPermittedDueToNullMentorOrMentee, result?.Error?.Message);
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
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { mentorEnrollment });

    var command = CreateDummyCommand();
    var handler = new AdminCreateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminCreateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipNotPermittedDueToNullMentorOrMentee, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
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
    _dbContextMock.Setup(x => x.Enrollments)
                  .ReturnsDbSet(new List<Enrollment> { menteeEnrollment, mentorEnrollment });
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<Mentorship>() );

    var command = CreateDummyCommand();
    var handler = new AdminCreateMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminCreateMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MentorshipCreatedSuccessfully, result.SuccessMessage);
  }
}
