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

public class AdminListMentorshipTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminListMentorshipTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminListMentorship.Command ListDummyCommand()
  {
    return new AdminListMentorship.Command();
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<MentorshipEntity>
                  {
                    new()
                    {
                      MentorshipId = DummyId1,
                      MentorEnrollmentId = DummyId1,
                      MenteeEnrollmentId = DummyId2,
                      MentorEnrollment = new EnrollmentEntity
                      {
                        EnrollmentId = DummyId1,
                        SeasonId = DummyId1,
                        UserId = DummyId1,
                        Season = new SeasonEntity
                        {
                          SeasonId = DummyId1,
                          Name = "Season Name"
                        },
                        Role = SeasonRole.Coordinator
                      },
                      MenteeEnrollment = new EnrollmentEntity
                      {
                        EnrollmentId = DummyId2,
                        SeasonId = DummyId2,
                        UserId = DummyId2,
                        Season = new SeasonEntity
                        {
                          SeasonId = DummyId1,
                          Name = "Season Name"
                        },
                        Role = SeasonRole.Mentor
                      }
                    }
                  });

    var command = ListDummyCommand();
    var handler =
      new AdminListMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminListMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MentorshipListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.Mentorships);
    Assert.Single(result.ResponseBody.Mentorships);

    var mentorships = result.ResponseBody.Mentorships.ToList();
    var mentorship = mentorships[0];
    Assert.Equal(DummyId1, mentorship.MentorshipId);
    Assert.Equal(DummyId1, mentorship.Mentor.EnrollmentId);
    Assert.Equal(DummyId2, mentorship.Mentee.EnrollmentId);
    Assert.Equal("Season Name", mentorship.Mentor.Season.Name);
    Assert.Equal("Season Name", mentorship.Mentee.Season.Name);
  }
}
