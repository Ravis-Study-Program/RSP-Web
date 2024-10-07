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

public class AdminDeleteMentorshipTests : TestsHelper
{
  private readonly Mock<ApplicationDbContext> _dbContextMock;

  public AdminDeleteMentorshipTests()
  {
    _dbContextMock = new Mock<ApplicationDbContext>();
  }

  private AdminDeleteMentorship.Command DeleteDummyCommand()
  {
    return new AdminDeleteMentorship.Command
    {
      MentorshipId = DummyGuid
    };
  }

  [Fact]
  public async Task Handle_MentorshipDoesNotExists_BadRequest()
  {
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<Mentorship>());

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    Assert.Equal(Message.MentorshipDoesNotExists, result?.Error?.Message);
  }

  [Fact]
  public async Task Handle_Success_OK()
  {
    var existingMentorship = new Mentorship
    {
      MentorshipId = DummyGuid, 
      MentorEnrollmentId = DummyGuid,
      MenteeEnrollmentId = DummyGuid2
    };
    _dbContextMock.Setup(x => x.Mentorships)
                  .ReturnsDbSet(new List<Mentorship> { existingMentorship });

    var command = DeleteDummyCommand();
    var handler = new AdminDeleteMentorship.Handler(_dbContextMock.Object, Mock.Of<ILogger<AdminDeleteMentorship.Handler>>());

    var result = await handler.Handle(command, CancellationToken.None);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    Assert.Equal(Message.MentorshipDeletedSuccessfully, result.SuccessMessage);
  }
}
