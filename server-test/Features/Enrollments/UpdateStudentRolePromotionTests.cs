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

public class UpdateStudentRolePromotionTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public UpdateStudentRolePromotionTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private UpdateStudentRolePromotion.Command DeleteDummyCommand()
  {
    return new UpdateStudentRolePromotion.Command
    {
      MenteeEnrollmentId = DummyId1,
      Email = DummyEmail,
      SeasonSlug = DummySlug,
      StudentRolePromotion = SeasonStudentRolePromotion.Intermediate,
    };
  }

  [Fact]
  public async Task Handle_CurrentUserEnrollmentDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Enrollments).ReturnsDbSet(new List<EnrollmentEntity>());

    var command = DeleteDummyCommand();
    var handler = new UpdateStudentRolePromotion.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<UpdateStudentRolePromotion.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UpdateStudentRolePromotionEnrollmentDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_CurrentUserIsNotMentorOrCoordinator_BadRequest()
  {
    var currentUserEnrollmentStudent = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Season = new SeasonEntity { Slug = DummySlug },
      Role = SeasonRole.Student,
      User = new UserEntity { Email = DummyEmail },
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Season = new SeasonEntity { Slug = DummySlug },
      User = new UserEntity { Email = "mentee@email.com" },
    };
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { currentUserEnrollmentStudent, menteeEnrollment });

    var command = DeleteDummyCommand();
    var handler = new UpdateStudentRolePromotion.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<UpdateStudentRolePromotion.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UpdateStudentRolePromotionEnrollmentDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_MentorshipDoesNotExist_BadRequest()
  {
    var currentUserEnrollmentMentor = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Season = new SeasonEntity { Slug = DummySlug },
      Role = SeasonRole.Mentor,
      User = new UserEntity { Email = DummyEmail },
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Season = new SeasonEntity { Slug = DummySlug },
      User = new UserEntity { Email = "mentee@email.com" },
      Role = SeasonRole.Student,
    };
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { currentUserEnrollmentMentor, menteeEnrollment });
    _dbContextMock.Setup(x => x.Mentorships).ReturnsDbSet(new List<MentorshipEntity>());

    var command = DeleteDummyCommand();
    var handler = new UpdateStudentRolePromotion.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<UpdateStudentRolePromotion.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_MenteeIsNotStudent_BadRequest()
  {
    var currentUserEnrollmentCoordinator = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Season = new SeasonEntity { Slug = DummySlug },
      Role = SeasonRole.Coordinator,
      User = new UserEntity { Email = DummyEmail },
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Season = new SeasonEntity { Slug = DummySlug },
      Role = SeasonRole.Mentor,
      User = new UserEntity { Email = "mentee@email.com" },
    };
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(
        new List<EnrollmentEntity> { currentUserEnrollmentCoordinator, menteeEnrollment }
      );

    var command = DeleteDummyCommand();
    var handler = new UpdateStudentRolePromotion.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<UpdateStudentRolePromotion.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.UpdateStudentRolePromotionMenteeDoesntExist, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_Coordinator_OK()
  {
    var currentUserEnrollmentCoordinator = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Season = new SeasonEntity { Slug = DummySlug },
      Role = SeasonRole.Coordinator,
      User = new UserEntity { Email = DummyEmail },
      StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Role = SeasonRole.Student,
      Season = new SeasonEntity { Slug = DummySlug },
      StudentRolePromotion = SeasonStudentRolePromotion.Beginner,
    };
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(
        new List<EnrollmentEntity> { currentUserEnrollmentCoordinator, menteeEnrollment }
      );

    var command = DeleteDummyCommand();
    var handler = new UpdateStudentRolePromotion.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<UpdateStudentRolePromotion.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.UpdateStudentRolePromotionSuccessfully, result.SuccessMessage);
  }

  [Fact]
  public async Task Handle_Success_Mentor_OK()
  {
    var currentUserEnrollmentMentor = new EnrollmentEntity
    {
      EnrollmentId = DummyId2,
      Season = new SeasonEntity { Slug = DummySlug },
      Role = SeasonRole.Mentor,
      User = new UserEntity { Email = DummyEmail },
      StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
    };
    var menteeEnrollment = new EnrollmentEntity
    {
      EnrollmentId = DummyId1,
      Season = new SeasonEntity { Slug = DummySlug },
      Role = SeasonRole.Student,
      User = new UserEntity { Email = "mentee@email.com" },
      StudentRolePromotion = SeasonStudentRolePromotion.Beginner,
    };
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(new List<EnrollmentEntity> { currentUserEnrollmentMentor, menteeEnrollment });

    var mentorship = new MentorshipEntity
    {
      MenteeEnrollmentId = menteeEnrollment.EnrollmentId,
      MentorEnrollment = currentUserEnrollmentMentor,
    };
    _dbContextMock
      .Setup(x => x.Mentorships)
      .ReturnsDbSet(new List<MentorshipEntity> { mentorship });

    var command = DeleteDummyCommand();
    var handler = new UpdateStudentRolePromotion.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<UpdateStudentRolePromotion.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.UpdateStudentRolePromotionSuccessfully, result.SuccessMessage);
  }
}
