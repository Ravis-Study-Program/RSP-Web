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
using RSPWebAPI.Features.ProblemAttempts;
using RSPWebAPI.Features.ProblemAttempts.Dtos;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Tests
{
  public class ProblemAttemptTests : BaseIntegrationTest, IAsyncLifetime
  {
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly TestDataSeeder _seeder;
    private readonly Faker _faker = new();
    private IProblemAttemptService ProblemAttemptService => GetService<IProblemAttemptService>();

    public ProblemAttemptTests(IntegrationTestWebAppFactory factory)
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
    public async Task Create_And_List_ProblemAttempts()
    {
      const int count = 2;
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);

      for (var i = 0; i < count; i++)
      {
        await _seeder.SeedProblemAttemptAsync(email);
      }

      var listRequest = new ListProblemAttemptRequest
      {
        Email = email,
        IncludeLeetcode = false,
        IncludeCustom = false,
      };

      var listResponse = await ProblemAttemptService.ListProblemAttempt(listRequest);
      Assert.True(listResponse.IsSuccess);
      Assert.Equal(Message.ProblemAttemptListSuccessfully, listResponse.Message);
      Assert.NotNull(listResponse.Data);
      Assert.Equal(count, listResponse.Data.ProblemAttempts.Count);
    }

    [Fact]
    public async Task CreateProblemAttempt_Succeeds()
    {
      var email = _faker.Internet.Email().ToLower();
      var userId = await _seeder.SeedUserAsync(email);
      var seasonId = await _seeder.SeedSeasonAndSeasonWeeks();
      var enrollmentId = await _seeder.SeedEnrollmentAsync(seasonId, userId);

      var request = new CreateProblemAttemptRequest
      {
        Email = email,
        EnrollmentId = enrollmentId,
        LeetcodeProblemId = null,
        CustomProblemId = null,
        AttemptStartDateUtc = DateTime.UtcNow.AddDays(2),
        TimeTakenInMinutes = 20,
        Notes = _faker.Lorem.Sentence(),
      };

      var response = await ProblemAttemptService.CreateProblemAttempt(request);
      Assert.Equal(Message.ProblemAttemptCreatedSuccessfully, response.Message);
      Assert.True(response.IsSuccess);
      Assert.NotNull(response.Data?.ProblemAttemptId);

      var all = await _seeder.GetAllProblemAttemptsAsync();
      Assert.Single(all);
    }

    [Fact]
    public async Task CreateProblemAttempt_Fails_If_DifferentEmail_In_Enrollment()
    {
      var user1Email = _faker.Internet.Email().ToLower();
      var user1Id = await _seeder.SeedUserAsync(user1Email);
      var enrollmentId = await _seeder.SeedEnrollmentAsync(userId: user1Id);

      var user2Email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(user2Email);

      var request = new CreateProblemAttemptRequest
      {
        Email = user2Email,
        EnrollmentId = enrollmentId,
        AttemptStartDateUtc = DateTime.UtcNow,
        TimeTakenInMinutes = 30,
      };
      var response = await ProblemAttemptService.CreateProblemAttempt(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.EnrollmentDoesNotExists, response.Message);
    }

    [Fact]
    public async Task CreateProblemAttempt_Fails_If_Email_NotFound()
    {
      var request = new CreateProblemAttemptRequest
      {
        Email = _faker.Internet.Email().ToLower(),
        TimeTakenInMinutes = 20,
      };
      var response = await ProblemAttemptService.CreateProblemAttempt(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.UserEmailDoesNotExists, response.Message);
    }

    [Fact]
    public async Task Update_ProblemAttempt_Should_Reflect_New_Values()
    {
      var email = _faker.Internet.Email().ToLower();
      var userId = await _seeder.SeedUserAsync(email);
      var seasonId = await _seeder.SeedSeasonAndSeasonWeeks();
      var enrollmentId = await _seeder.SeedEnrollmentAsync(seasonId, userId);

      var attemptId = await _seeder.SeedProblemAttemptAsync(email, enrollmentId);

      var existing = await ProblemAttemptService.GetProblemAttemptByIdAsync(attemptId);
      Assert.NotNull(existing);

      var updateRequest = new UpdateProblemAttemptRequest
      {
        ProblemAttemptId = attemptId,
        EnrollmentId = existing!.EnrollmentId,
        AttemptStartDateUtc = DateTime.UtcNow.AddDays(5),
        TimeTakenInMinutes = 99,
        Notes = _faker.Lorem.Sentence(),
        LeetcodeProblemId = null,
        CustomProblemId = null,
      };

      var updateResponse = await ProblemAttemptService.UpdateProblemAttempt(updateRequest);
      Assert.True(updateResponse.IsSuccess);
      Assert.Equal(Message.ProblemAttemptUpdatedSuccessfully, updateResponse.Message);

      var updated = await ProblemAttemptService.GetProblemAttemptByIdAsync(attemptId);
      Assert.NotNull(updated);
      Assert.Equal(updateRequest.AttemptStartDateUtc, updated!.AttemptStartDateUtc);
      Assert.Equal(updateRequest.TimeTakenInMinutes, updated.TimeTakenInMinutes);
      Assert.Equal(updateRequest.Notes, updated.Notes);
    }

    [Fact]
    public async Task Update_ProblemAttempt_Fails_If_Not_Found()
    {
      var request = new UpdateProblemAttemptRequest
      {
        ProblemAttemptId = _faker.Random.AlphaNumeric(10),
        EnrollmentId = _faker.Random.AlphaNumeric(10),
      };
      var response = await ProblemAttemptService.UpdateProblemAttempt(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.ProblemAttemptDoesNotExists, response.Message);
    }

    [Fact]
    public async Task Delete_ProblemAttempt_Removes_It()
    {
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);
      var attemptId = await _seeder.SeedProblemAttemptAsync(email);

      var request = new DeleteProblemAttemptRequest { ProblemAttemptId = attemptId, Email = email };
      var deleteResponse = await ProblemAttemptService.DeleteProblemAttempt(request);
      Assert.True(deleteResponse.IsSuccess);
      Assert.Equal(Message.ProblemAttemptDeletedSuccessfully, deleteResponse.Message);

      var afterDelete = await ProblemAttemptService.GetProblemAttemptByIdAsync(attemptId);
      Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Delete_ProblemAttempt_Fails_If_Wrong_Email()
    {
      var email1 = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email1);
      var attemptId = await _seeder.SeedProblemAttemptAsync(email1);

      var email2 = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email2);

      var request = new DeleteProblemAttemptRequest
      {
        ProblemAttemptId = attemptId,
        Email = email2,
      };
      var response = await ProblemAttemptService.DeleteProblemAttempt(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.ProblemAttemptDoesNotExists, response.Message);
    }

    [Fact]
    public async Task Delete_ProblemAttempt_Fails_If_NotFound()
    {
      var request = new DeleteProblemAttemptRequest
      {
        ProblemAttemptId = _faker.Random.AlphaNumeric(10),
        Email = _faker.Internet.Email().ToLower(),
      };
      var response = await ProblemAttemptService.DeleteProblemAttempt(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.ProblemAttemptDoesNotExists, response.Message);
    }

    [Fact]
    public async Task ListProblemAttempt_Succeeds()
    {
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);
      await _seeder.SeedProblemAttemptAsync(email);

      var listRequest = new ListProblemAttemptRequest
      {
        Email = email,
        IncludeLeetcode = true,
        IncludeCustom = true,
      };

      var listResponse = await ProblemAttemptService.ListProblemAttempt(listRequest);
      Assert.True(listResponse.IsSuccess);
      Assert.Equal(Message.ProblemAttemptListSuccessfully, listResponse.Message);
      Assert.NotNull(listResponse.Data);
      Assert.NotEmpty(listResponse.Data.ProblemAttempts);
    }
  }
}
