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

public class GetCurrentUserMenteesListTests: TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public GetCurrentUserMenteesListTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private GetCurrentUserMenteesList.Command ListDummyCommand()
  {
    return new GetCurrentUserMenteesList.Command
    {
      Email = DummyEmail,
      SeasonSlug = DummySlug
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
                          Slug = DummySlug,
                        },
                        Role = new Role()
                        {
                          RoleId = DummyGuid,
                          Name = "Mentor Role Name"
                        },
                        User = new User()
                        {
                          UserId = DummyGuid,
                          Email = DummyEmail
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
                          Slug = DummySlug
                        },
                        Role = new Role
                        {
                          RoleId = DummyGuid,
                          Name = "Mentee Role Name"
                        },
                        User = new User
                        {
                          UserId = DummyGuid2,
                          Email = "mentee_email@gmail.com",
                          Name = DummyName
                        }
                      }
                    }
                  });

    var command = ListDummyCommand();
    var handler = new GetCurrentUserMenteesList.Handler(_dbContextMock.Object, Mock.Of<ILogger<GetCurrentUserMenteesList.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MentorshipListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.Mentees);
    Assert.Single(result.ResponseBody.Mentees);

    var mentorships = result.ResponseBody.Mentees.ToList();
    var mentorship = mentorships[0];
    Assert.Equal(DummyName, mentorship.MenteeEnrollment.User.Name);
    Assert.Equal("mentee_email@gmail.com", mentorship.MenteeEnrollment.User.Email);
    Assert.Equal("Season Name", mentorship.MenteeEnrollment.Season.Name);
    Assert.Equal(DummyGuid, mentorship.MenteeEnrollment.Season.SeasonId);

  }
}
