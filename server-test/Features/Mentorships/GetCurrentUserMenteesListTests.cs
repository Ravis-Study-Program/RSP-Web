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

public class GetCurrentUserMenteesListTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public GetCurrentUserMenteesListTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private GetCurrentUserMenteesList.Command ListDummyCommand()
  {
    return new GetCurrentUserMenteesList.Command { Email = DummyEmail, SeasonSlug = DummySlug };
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    _dbContextMock
      .Setup(x => x.Mentorships)
      .ReturnsDbSet(
        new List<MentorshipEntity>
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
              Season = new SeasonEntity { SeasonId = DummyId1, Slug = DummySlug },
              User = new UserEntity { UserId = DummyId1, Email = DummyEmail },
            },
            MenteeEnrollment = new EnrollmentEntity
            {
              EnrollmentId = DummyId2,
              SeasonId = DummyId2,
              UserId = DummyId2,
              Season = new SeasonEntity
              {
                SeasonId = DummyId1,
                Name = "Season Name",
                Slug = DummySlug,
              },
              User = new UserEntity { UserId = DummyId2, Name = DummyName },
            },
          },
        }
      );

    var command = ListDummyCommand();
    var handler = new GetCurrentUserMenteesList.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<GetCurrentUserMenteesList.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MentorshipListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.Mentees);
    Assert.Single(result.ResponseBody.Mentees);

    var mentorships = result.ResponseBody.Mentees.ToList();
    var mentorship = mentorships[0];
    Assert.Equal(DummyName, mentorship.MenteeName);
    Assert.Equal(DummyId2, mentorship.MenteeEnrollmentId);
  }
}
