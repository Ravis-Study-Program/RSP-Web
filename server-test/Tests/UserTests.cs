using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Bogus;
using Moq;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.Mentorships.Interfaces;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Dtos;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Tests;

public class UserTests : BaseIntegrationTest, IAsyncLifetime
{
  private readonly IntegrationTestWebAppFactory _factory;
  private readonly TestDataSeeder _seeder;
  private readonly Faker _faker = new();
  private IUserService UserService => GetService<IUserService>();

  public UserTests(IntegrationTestWebAppFactory factory)
    : base(factory)
  {
    _factory = factory;
    _seeder = new TestDataSeeder(
      DbContext,
      GetService<IUserService>(),
      GetService<ISeasonService>(),
      GetService<ISeasonWeekService>(),
      GetService<IEnrollmentService>(),
      GetService<IProblemAttemptService>(),
      GetService<IMockInterviewService>(),
      GetService<IMentorshipService>(),
      GetService<IRepository<LeetcodeProblemEntity>>(),
      GetService<IRepository<LeetcodeProblemRecommendationEntity>>()
    );
  }

  public async Task InitializeAsync()
  {
    await _factory.ResetDatabase();
  }

  public Task DisposeAsync() => Task.CompletedTask;

  [Fact]
  public async Task Create_And_List_Users()
  {
    const int count = 3;
    for (var i = 0; i < count; i++)
    {
      await _seeder.SeedUserAsync();
    }

    var users = await _seeder.GetAllUsersAsync();
    Assert.Equal(count, users.Count);
  }

  [Fact]
  public async Task Update_User_Should_Reflect_New_Values()
  {
    await _seeder.SeedUserAsync();
    var users = await _seeder.GetAllUsersAsync();
    Assert.NotEmpty(users);

    var targetUser = users.First();
    var updateRequest = new AdminUpdateUserRequest
    {
      UserId = targetUser.UserId,
      Email = targetUser.Email,
      Name = _faker.Name.FullName(),
      ProfileImage = _faker.Image.PicsumUrl(),
      DiscordId = _faker.Random.AlphaNumeric(8),
      IsAdmin = false,
    };

    var updateResponse = await UserService.UpdateAdminUser(updateRequest);
    Assert.True(updateResponse.IsSuccess);
    Assert.Equal(Messages.User.Updated, updateResponse.Message);

    var updatedUser = await UserService.GetUserByIdAsync(targetUser.UserId);
    Assert.NotNull(updatedUser);
    Assert.Equal(updateRequest.Name, updatedUser!.Name);
    Assert.Equal(updateRequest.ProfileImage, updatedUser.ProfileImage);
    Assert.Equal(updateRequest.DiscordId, updatedUser.DiscordId);
    Assert.Equal(updateRequest.IsAdmin, updatedUser.IsAdmin);
  }

  [Fact]
  public async Task Delete_User_Should_Remove_It_From_List()
  {
    const int count = 3;
    for (var i = 0; i < count; i++)
    {
      await _seeder.SeedUserAsync();
    }

    var users = await _seeder.GetAllUsersAsync();
    Assert.Equal(count, users.Count);

    var targetUser = users.Last();
    var deleteRequest = new AdminDeleteUserRequest { Email = targetUser.Email };
    var deleteResponse = await UserService.DeleteAdminUser(deleteRequest);
    Assert.True(deleteResponse.IsSuccess);
    Assert.Equal(Messages.User.Deleted, deleteResponse.Message);

    var deletedUser = await UserService.GetUserByEmailAsync(targetUser.Email);
    Assert.Null(deletedUser);

    users = await _seeder.GetAllUsersAsync();
    Assert.Equal(count - 1, users.Count);
  }

  [Fact]
  public async Task CreateUserIfNotExists_Only_Creates_One_User_And_Sends_Verification_Email()
  {
    var mock = _factory.MockUserIdentityService;
    var request = new CreateUserIfNotExistsRequest { Name = _faker.Name.FullName() };
    var email = _faker.Internet.Email();

    mock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
      .ReturnsAsync(
        new List<User>
        {
          new User { EmailVerified = false, UserId = "auth0|123" },
        }
      );

    mock.Setup(x => x.SendVerificationEmailAsync("auth0|123"))
      .Returns(Task.CompletedTask)
      .Verifiable();

    var firstResponse = await UserService.CreateUserIfNotExists(request, email);
    Assert.Equal(Messages.User.Created, firstResponse.Message);
    Assert.True(firstResponse.IsSuccess);

    var secondResponse = await UserService.CreateUserIfNotExists(request, email);
    Assert.True(secondResponse.IsSuccess);
    Assert.Equal(Messages.User.Created, secondResponse.Message);

    var users = await _seeder.GetAllUsersAsync();
    Assert.Single(users.Where(u => u.Email == email));

    mock.Verify(x => x.SendVerificationEmailAsync("auth0|123"), Times.Exactly(2));
  }

  [Fact]
  public async Task CreateUserIfNotExists_Dont_Send_Verification_Email_If_Verified()
  {
    var mock = _factory.MockUserIdentityService;
    var email = _faker.Internet.Email();
    var request = new CreateUserIfNotExistsRequest { Name = _faker.Name.FullName() };

    mock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
      .ReturnsAsync(
        new List<User>
        {
          new User { EmailVerified = true, UserId = "auth0|123" },
        }
      );

    mock.Verify(x => x.SendVerificationEmailAsync("auth0|123"), Times.Never());

    var result = await UserService.CreateUserIfNotExists(request, email);

    mock.Verify(x => x.SendVerificationEmailAsync("auth0|123"), Times.Never());
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task CreateUserIfNotExists_Link_Account_If_Other_Login_Method_Exists()
  {
    var mock = _factory.MockUserIdentityService;
    var email = "test@gmail.com";
    var request = new CreateUserIfNotExistsRequest { Name = _faker.Name.FullName() };
    await _seeder.SeedUserAsync(email);

    var user1 = new User
    {
      EmailVerified = true,
      Email = email,
      UserId = "auth0|123",
      LoginsCount = "1",
      Identities = new[]
      {
        new Identity { Provider = "auth0", UserId = "auth0|123" },
      },
    };

    var user2 = new User
    {
      EmailVerified = true,
      Email = email,
      UserId = "google|123",
      LoginsCount = "2",
      Identities = new[]
      {
        new Identity { Provider = "google", UserId = "google|123" },
      },
    };

    mock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User> { user1, user2 });

    var result = await UserService.CreateUserIfNotExists(request, email);

    mock.Verify(x => x.LinkAccountAsync(user1.UserId, user2), Times.Once());
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task GetCurrentUser_Returns_Correct_User()
  {
    var email = _faker.Internet.Email();
    await _seeder.SeedUserAsync(email);

    var currentUserResponse = await UserService.GetCurrentUser(email);
    Assert.True(currentUserResponse.IsSuccess);
    Assert.Equal(Messages.User.EmailExists, currentUserResponse.Message);

    var currentUser = currentUserResponse.Data?.User;
    Assert.NotNull(currentUser);
    Assert.Equal(email, currentUser!.Email);
  }
}
