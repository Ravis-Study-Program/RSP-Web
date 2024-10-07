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

public class AdminListMentorshipTests: TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminListMentorshipTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminListMentorship.Command ListDummyCommand()
  {
    return new AdminListMentorship.Command
    {
    };
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<Mentorship>
                  {
                    new Mentorship
                    {
                      MentorshipId = DummyGuid,
                      MentorEnrollmentId = DummyGuid,
                      MenteeEnrollmentId = DummyGuid2,
                      MentorEnrollment = new Enrollment
                      {
                        EnrollmentId = DummyGuid,
                        SeasonId = DummyGuid,
                        UserId = DummyGuid,
                        RoleId = DummyGuid,
                        Season = new Season
                        {
                          SeasonId = DummyGuid,
                          Name = "Season Name",
                        },
                        Role = new Role()
                        {
                          RoleId = DummyGuid,
                          Name = "Mentor Role Name"
                        }
                      },
                      MenteeEnrollment = new Enrollment
                      {
                        EnrollmentId = DummyGuid2,
                        SeasonId = DummyGuid2,
                        UserId = DummyGuid2,
                        RoleId = DummyGuid2,
                        Season = new Season
                        {
                          SeasonId = DummyGuid,
                          Name = "Season Name",
                        },
                        Role = new Role
                        {
                          RoleId = DummyGuid,
                          Name = "Mentee Role Name"
                        }
                      }
                    }
                  });

    var command = ListDummyCommand();
    var handler = new AdminListMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminListMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MentorshipListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.Mentorships);
    Assert.Single(result.ResponseBody.Mentorships);

    var mentorships = result.ResponseBody.Mentorships.ToList();
    var mentorship = mentorships[0];
    Assert.Equal(DummyGuid, mentorship.MentorshipId);
    Assert.Equal(DummyGuid, mentorship.Mentor.EnrollmentId);
    Assert.Equal(DummyGuid2, mentorship.Mentee.EnrollmentId);
    Assert.Equal("Mentor Role Name", mentorship.Mentor.Role.Name);
    Assert.Equal("Mentee Role Name", mentorship.Mentee.Role.Name);
    Assert.Equal("Season Name", mentorship.Mentor.Season.Name);
    Assert.Equal("Season Name", mentorship.Mentee.Season.Name);
  }
}
