using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
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
    Assert.Equal(Message.UserUpdatedSuccessfully, updateResponse.Message);

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
    Assert.Equal(Message.UserDeletedSuccessfully, deleteResponse.Message);

    var deletedUser = await UserService.GetUserByEmailAsync(targetUser.Email);
    Assert.Null(deletedUser);

    users = await _seeder.GetAllUsersAsync();
    Assert.Equal(count - 1, users.Count);
  }

  [Fact]
  public async Task CreateUserIfNotExists_Only_Creates_One_User()
  {
    var request = new CreateUserIfNotExistsRequest
    {
      DiscordId = _faker.Random.AlphaNumeric(8),
      ProfileImage = _faker.Image.PicsumUrl(),
      Name = _faker.Name.FullName(),
    };

    var email = _faker.Internet.Email();
    var firstResponse = await UserService.CreateUserIfNotExists(request, email);
    Assert.True(firstResponse.IsSuccess);
    Assert.Equal(Message.UserCreatedSuccessfully, firstResponse.Message);

    var secondResponse = await UserService.CreateUserIfNotExists(request, email);
    Assert.False(secondResponse.IsSuccess);
    Assert.Equal(Message.UserEmailExists, secondResponse.Message);

    var users = await _seeder.GetAllUsersAsync();
    Assert.Single(users.Where(u => u.Email == email));
  }

  [Fact]
  public async Task GetCurrentUser_Returns_Correct_User()
  {
    var email = _faker.Internet.Email();
    await _seeder.SeedUserAsync(email);

    var currentUserResponse = await UserService.GetCurrentUser(email);
    Assert.True(currentUserResponse.IsSuccess);
    Assert.Equal(Message.UserEmailExists, currentUserResponse.Message);

    var currentUser = currentUserResponse.Data?.User;
    Assert.NotNull(currentUser);
    Assert.Equal(email, currentUser!.Email);
  }

  [Fact]
  public async Task GetGraduates_Returns_Expected_Count()
  {
    const int count = 3;
    for (var i = 0; i < count; i++)
    {
      await _seeder.SeedUserAsync();
    }

    var gradsResponse = await UserService.GetGraduates(new GetGraduatesRequest());
    Assert.True(gradsResponse.IsSuccess);
    Assert.Equal(Message.GraduatesListSuccessfully, gradsResponse.Message);

    var graduates = gradsResponse.Data?.Graduates.ToList();
    Assert.NotNull(graduates);
    Assert.Equal(count, graduates!.Count);
  }
}
