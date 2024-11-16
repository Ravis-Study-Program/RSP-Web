using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.SeasonUsers;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Features.SeasonUsers;

public class GetSeasonUsersTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public GetSeasonUsersTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private GetSeasonUsers.Command ListDummyCommand()
  {
    return new GetSeasonUsers.Command { SeasonSlug = DummySlug };
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    const string discordId1 = "Discord ID 123";
    const string discordId2 = "Discord ID 456";
    _dbContextMock
      .Setup(x => x.Enrollments)
      .ReturnsDbSet(
        new List<EnrollmentEntity>
        {
          new()
          {
            User = new UserEntity
            {
              UserId = DummyId1,
              DiscordId = discordId1,
              Email = DummyEmail,
              Name = DummyName,
              ProfileImage = DummyProfileImage,
            },
            Role = SeasonRole.Coordinator,
            Season = new SeasonEntity { Slug = DummySlug },
          },
          new()
          {
            User = new UserEntity
            {
              UserId = DummyId2,
              DiscordId = discordId2,
              Email = DummyEmail,
              Name = DummyName,
              ProfileImage = DummyProfileImage,
            },
            Role = SeasonRole.Student,
            Season = new SeasonEntity { Slug = "Another Season Slug" },
          },
        }
      );

    var command = ListDummyCommand();
    var handler = new GetSeasonUsers.Handler(
      _dbContextMock.Object,
      Mock.Of<ILogger<GetSeasonUsers.Handler>>()
    );

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.SeasonUsersListSuccessfully, result.SuccessMessage);
    Assert.NotNull(result.ResponseBody?.SeasonUsers);
    Assert.Single(result.ResponseBody.SeasonUsers);

    var graduates = result.ResponseBody.SeasonUsers.ToList();
    Assert.Equal(discordId1, graduates[0].DiscordId);
    Assert.Equal(DummyProfileImage, graduates[0].ProfileImage);
    Assert.Equal(DummyName, graduates[0].Name);
    Assert.Equal(SeasonRole.Coordinator, graduates[0].Role);
  }
}
