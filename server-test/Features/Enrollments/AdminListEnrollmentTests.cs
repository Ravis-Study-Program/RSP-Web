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

public class AdminListEnrollmentTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminListEnrollmentTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminListEnrollment.Command ListDummyCommand()
  {
    return new AdminListEnrollment.Command();
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(
        new List<EnrollmentEntity>
        {
          new()
          {
            EnrollmentId = DummyId1,
            SeasonId = DummyId1,
            UserId = DummyId1,
            Season = new SeasonEntity
            {
              SeasonId = DummyId1,
              Name = "Season Name",
              Location = DummyLocation,
            },
            User = new UserEntity { Name = "User Name", Email = DummyEmail },
            Role = SeasonRole.Mentor,
            StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
          },
        }
      );

    var command = ListDummyCommand();
    var handler = new AdminListEnrollment.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<AdminListEnrollment.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.EnrollmentListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.Enrollments);
    Assert.Single(result.ResponseBody.Enrollments);

    var enrollments = result.ResponseBody.Enrollments.ToList();
    var enrollment = enrollments[0];
    Assert.Equal(DummyId1, enrollment.EnrollmentId);
    Assert.Equal("User Name", enrollment.UserName);
    Assert.Equal("Season Name", enrollment.SeasonName);
    Assert.Equal(SeasonRole.Mentor, enrollment.Role);
    Assert.Equal(SeasonStudentRolePromotion.NotApplicable, enrollment.StudentRolePromotion);
  }
}
