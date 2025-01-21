using System;
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
using RSPWebAPI.Features.SeasonWeeks;
using RSPWebAPI.Features.SeasonWeeks.Dtos;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Tests
{
  public class SeasonWeekTests : BaseIntegrationTest, IAsyncLifetime
  {
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly TestDataSeeder _seeder;
    private readonly Faker _faker = new();
    private ISeasonWeekService SeasonWeekService => GetService<ISeasonWeekService>();
    private ISeasonService SeasonService => GetService<ISeasonService>();

    public SeasonWeekTests(IntegrationTestWebAppFactory factory)
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
    public async Task Create_And_List_SeasonWeeks()
    {
      const int count = 2;
      var seasonId = await _seeder.SeedSeasonAsync();
      for (var i = 1; i <= count; i++)
      {
        await _seeder.SeedSeasonWeekAsync(seasonId, i);
      }
      var listRequest = new AdminListSeasonWeekRequest();
      var listResponse = await SeasonWeekService.ListAdminSeasonWeek(listRequest);
      Assert.True(listResponse.IsSuccess);
      Assert.Equal(Message.SeasonWeekListSuccessfully, listResponse.Message);
      var allWeeks = await _seeder.GetAllSeasonWeeksAsync();
      Assert.Equal(count, allWeeks.Count);
    }

    [Fact]
    public async Task Create_SeasonWeek_Fails_If_Season_NotFound()
    {
      var request = new AdminCreateSeasonWeekRequest
      {
        SeasonId = _faker.Random.AlphaNumeric(10),
        WeekNumber = 1,
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddDays(6),
      };
      var response = await SeasonWeekService.CreateAdminSeasonWeek(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.SeasonDoesNotExists, response.Message);
    }

    [Fact]
    public async Task Create_SeasonWeek_Fails_If_WeekNumber_Already_Exists()
    {
      var seasonId = await _seeder.SeedSeasonAsync(
        null,
        DateTime.UtcNow,
        DateTime.UtcNow.AddDays(10)
      );
      var firstRequest = new AdminCreateSeasonWeekRequest
      {
        SeasonId = seasonId,
        WeekNumber = 1,
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddDays(6),
      };
      var firstResponse = await SeasonWeekService.CreateAdminSeasonWeek(firstRequest);
      Assert.True(firstResponse.IsSuccess);
      Assert.Equal(Message.SeasonWeekCreatedSuccessfully, firstResponse.Message);
      var secondRequest = new AdminCreateSeasonWeekRequest
      {
        SeasonId = seasonId,
        WeekNumber = 1,
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddDays(6),
      };
      var secondResponse = await SeasonWeekService.CreateAdminSeasonWeek(secondRequest);
      Assert.False(secondResponse.IsSuccess);
      Assert.Equal(Message.SeasonWeekExists, secondResponse.Message);
    }

    [Fact]
    public async Task Create_SeasonWeek_Fails_If_Dates_Out_Of_Range()
    {
      var seasonId = await _seeder.SeedSeasonAsync();
      var season = await SeasonService.GetSeasonByIdAsync(seasonId);
      var request = new AdminCreateSeasonWeekRequest
      {
        SeasonId = seasonId,
        WeekNumber = 2,
        StartDate = season!.EndDateInclusiveUtc.AddDays(1),
        EndDate = season.EndDateInclusiveUtc.AddDays(2),
      };
      var response = await SeasonWeekService.CreateAdminSeasonWeek(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.SeasonWeekDatesNotWithinSeasonDates, response.Message);
    }

    [Fact]
    public async Task Update_SeasonWeek_Should_Reflect_New_Values()
    {
      var seasonId = await _seeder.SeedSeasonAsync();
      var seasonWeekId = await _seeder.SeedSeasonWeekAsync(seasonId, 1);
      var updateRequest = new AdminUpdateSeasonWeekRequest
      {
        SeasonWeekId = seasonWeekId,
        WeekNumber = 5,
        StartDate = DateTime.UtcNow.AddDays(1),
        EndDate = DateTime.UtcNow.AddDays(7),
      };
      var updateResponse = await SeasonWeekService.UpdateAdminSeasonWeek(updateRequest);
      Assert.True(updateResponse.IsSuccess);
      Assert.Equal(Message.SeasonWeekUpdateSuccessfully, updateResponse.Message);
      var updated = await SeasonWeekService.GetSeasonWeekByIdAsync(seasonWeekId);
      Assert.NotNull(updated);
      Assert.Equal(updateRequest.WeekNumber, updated!.WeekNumber);
      Assert.Equal(updateRequest.StartDate, updated.StartDate);
      Assert.Equal(updateRequest.EndDate, updated.EndDate);
    }

    [Fact]
    public async Task Delete_SeasonWeek_Should_Remove_It()
    {
      var seasonId = await _seeder.SeedSeasonAsync();
      var seasonWeekId = await _seeder.SeedSeasonWeekAsync(seasonId, 1);
      var request = new AdminDeleteSeasonWeekRequest { SeasonWeekId = seasonWeekId };
      var deleteResponse = await SeasonWeekService.DeleteAdminSeasonWeek(request);
      Assert.True(deleteResponse.IsSuccess);
      Assert.Equal(Message.SeasonWeekDeletedSuccessfully, deleteResponse.Message);
      var afterDelete = await SeasonWeekService.GetSeasonWeekByIdAsync(seasonWeekId);
      Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Delete_SeasonWeek_Fails_If_Not_Found()
    {
      var request = new AdminDeleteSeasonWeekRequest
      {
        SeasonWeekId = _faker.Random.AlphaNumeric(10),
      };
      var response = await SeasonWeekService.DeleteAdminSeasonWeek(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.SeasonWeekDoesNotExists, response.Message);
    }

    [Fact]
    public async Task ListAdminSeasonWeek_Should_Return_Success()
    {
      var listRequest = new AdminListSeasonWeekRequest();
      var listResponse = await SeasonWeekService.ListAdminSeasonWeek(listRequest);
      Assert.True(listResponse.IsSuccess);
      Assert.Equal(Message.SeasonWeekListSuccessfully, listResponse.Message);
    }
  }
}
