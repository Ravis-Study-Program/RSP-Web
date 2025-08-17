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
using RSPWebAPI.Features.Seasons;
using RSPWebAPI.Features.Seasons.Dtos;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Tests;

public class SeasonTests : BaseIntegrationTest, IAsyncLifetime
{
  private readonly IntegrationTestWebAppFactory _factory;
  private readonly TestDataSeeder _seeder;
  private readonly Faker _faker = new();
  private ISeasonService SeasonService => GetService<ISeasonService>();

  public SeasonTests(IntegrationTestWebAppFactory factory)
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
  public async Task Create_And_List_Seasons()
  {
    const int count = 2;
    for (var i = 0; i < count; i++)
    {
      await _seeder.SeedSeasonAsync();
    }

    var listRequest = new AdminListSeasonRequest();
    var listResponse = await SeasonService.ListAdminSeason(listRequest);
    Assert.True(listResponse.IsSuccess);
    Assert.Equal(Messages.Season.Listed, listResponse.Message);

    var allSeasons = await _seeder.GetAllSeasonsAsync();
    Assert.Equal(count, allSeasons.Count);
  }

  [Fact]
  public async Task Create_Season_Fails_If_Slug_Already_Exists()
  {
    var slug = _faker.Name.FirstName();
    var firstRequest = new AdminCreateSeasonRequest
    {
      Name = _faker.Name.FirstName(),
      Slug = slug,
      StartDateInclusiveUtc = DateTime.UtcNow,
      EndDateInclusiveUtc = DateTime.UtcNow.AddDays(7),
      Location = _faker.Address.City(),
      ImageUrl = _faker.Image.PicsumUrl(),
    };

    var firstResponse = await SeasonService.CreateAdminSeason(firstRequest);
    Assert.True(firstResponse.IsSuccess);
    Assert.Equal(Messages.Season.Created, firstResponse.Message);

    var secondRequest = new AdminCreateSeasonRequest
    {
      Name = _faker.Name.FirstName(),
      Slug = slug, // same slug
      StartDateInclusiveUtc = DateTime.UtcNow,
      EndDateInclusiveUtc = DateTime.UtcNow.AddDays(7),
      Location = _faker.Address.City(),
      ImageUrl = _faker.Image.PicsumUrl(),
    };

    var secondResponse = await SeasonService.CreateAdminSeason(secondRequest);
    Assert.False(secondResponse.IsSuccess);
    Assert.Equal(Messages.Season.Exists, secondResponse.Message);
  }

  [Fact]
  public async Task Update_Season_Should_Reflect_New_Values()
  {
    var seasonId = await _seeder.SeedSeasonAsync();
    var existingSeason = await SeasonService.GetSeasonByIdAsync(seasonId);
    Assert.NotNull(existingSeason);

    var updateRequest = new AdminUpdateSeasonRequest
    {
      SeasonId = existingSeason!.SeasonId,
      Name = _faker.Name.FirstName(),
      Slug = _faker.Name.FirstName(),
      StartDateInclusiveUtc = DateTime.UtcNow,
      EndDateInclusiveUtc = DateTime.UtcNow.AddDays(7),
      Location = _faker.Address.City(),
      ImageUrl = _faker.Image.PlaceImgUrl(),
    };

    var updateResponse = await SeasonService.UpdateAdminSeason(updateRequest);
    Assert.Equal(Messages.Season.Updated, updateResponse.Message);
    Assert.True(updateResponse.IsSuccess);

    var updatedSeason = await SeasonService.GetSeasonByIdAsync(seasonId);
    Assert.NotNull(updatedSeason);
    Assert.Equal(updateRequest.Name, updatedSeason!.Name);
    Assert.Equal(updateRequest.Slug, updatedSeason.Slug);
    Assert.Equal(updateRequest.Location, updatedSeason.Location);
    Assert.Equal(updateRequest.ImageUrl, updatedSeason.ImageUrl);
    Assert.Equal(updateRequest.StartDateInclusiveUtc, updatedSeason.StartDateInclusiveUtc);
    Assert.Equal(updateRequest.EndDateInclusiveUtc, updatedSeason.EndDateInclusiveUtc);
  }

  [Fact]
  public async Task Delete_Season_Should_Remove_It()
  {
    var seasonId = await _seeder.SeedSeasonAsync();
    var season = await SeasonService.GetSeasonByIdAsync(seasonId);
    Assert.NotNull(season);

    var request = new AdminDeleteSeasonRequest { SeasonId = season.SeasonId };
    var deleteResponse = await SeasonService.DeleteAdminSeason(request);
    Assert.True(deleteResponse.IsSuccess);
    Assert.Equal(Messages.Season.Deleted, deleteResponse.Message);

    var afterDelete = await SeasonService.GetSeasonByIdAsync(seasonId);
    Assert.Null(afterDelete);
  }

  [Fact]
  public async Task Delete_Season_Fails_If_Not_Found()
  {
    var request = new AdminDeleteSeasonRequest { SeasonId = _faker.Random.AlphaNumeric(12) };
    var deleteResponse = await SeasonService.DeleteAdminSeason(request);
    Assert.False(deleteResponse.IsSuccess);
    Assert.Equal(Messages.Season.DoesNotExist, deleteResponse.Message);
  }

  [Fact]
  public async Task ListAdminSeason_Should_Return_Success()
  {
    var request = new AdminListSeasonRequest();
    var response = await SeasonService.ListAdminSeason(request);
    Assert.True(response.IsSuccess);
    Assert.Equal(Messages.Season.Listed, response.Message);
  }
}
