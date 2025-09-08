using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.Mentorships.Interfaces;
using RSPWebAPI.Features.MockInterviews;
using RSPWebAPI.Features.MockInterviews.Dtos;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Tests
{
  public class MockInterviewTests : BaseIntegrationTest, IAsyncLifetime
  {
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly TestDataSeeder _seeder;
    private readonly Faker _faker = new();
    private IMockInterviewService MockInterviewService => GetService<IMockInterviewService>();

    public MockInterviewTests(IntegrationTestWebAppFactory factory)
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
    public async Task Create_And_List_MockInterviews()
    {
      const int count = 2;
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);

      for (var i = 0; i < count; i++)
      {
        await _seeder.SeedMockInterviewAsync(email);
      }

      var listRequest = new ListMockInterviewRequest
      {
        Emails = new List<string> { email },
        IncludeBehavioural = false,
        IncludeLeetcode = false,
        IncludeCustom = false,
      };
      var listResponse = await MockInterviewService.ListMockInterview(listRequest);
      Assert.NotNull(listResponse);
      Assert.NotNull(listResponse.MockInterviews);
      Assert.Equal(count, listResponse.MockInterviews.Count);
    }

    [Fact]
    public async Task CreateMockInterview_Succeeds()
    {
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);
      var mockInterviewId = await _seeder.SeedMockInterviewAsync(email);
      var all = await _seeder.GetAllMockInterviewsAsync();
      Assert.Single(all);
      Assert.Equal(mockInterviewId, all.First().MockInterviewId);
    }

    [Fact]
    public async Task CreateMockInterview_Fails_If_EnrollmentEmail_Mismatch()
    {
      var email1 = _faker.Internet.Email().ToLower();
      var email2 = _faker.Internet.Email().ToLower();
      var user1Id = await _seeder.SeedUserAsync(email1);
      var seasonId = await _seeder.SeedSeasonAsync();
      await _seeder.SeedUserAsync(email2);
      await _seeder.SeedEnrollmentAsync(userId: user1Id);

      var request = new CreateMockInterviewRequest
      {
        InterviewerEmail = email2,
        IntervieweeUserId = user1Id,
        SeasonId = seasonId,
        StartDate = DateTime.UtcNow,
      };
      await Assert.ThrowsAsync<KeyNotFoundException>(
        () => MockInterviewService.CreateMockInterview(request)
      );
    }

    [Fact]
    public async Task Update_MockInterview_Should_Reflect_New_Values()
    {
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);
      var mockInterviewId = await _seeder.SeedMockInterviewAsync(email);
      var existing = await MockInterviewService.GetMockInterviewByIdAsync(mockInterviewId);
      Assert.NotNull(existing);

      var newUserEmail = _faker.Internet.Email().ToLower();
      var newUserId = await _seeder.SeedUserAsync(newUserEmail);

      var updateRequest = new UpdateMockInterviewRequest
      {
        MockInterviewId = mockInterviewId,
        SeasonId = existing.SeasonId,
        IntervieweeUserId = newUserId,
        InterviewerEmail = email,
        StartDate = DateTime.UtcNow.AddDays(2),
        TimeTakenInMinutes = 90,
        MockInterviewRounds = new(),
      };

      var updateResp = await MockInterviewService.UpdateMockInterview(updateRequest);
      Assert.NotNull(updateResp);

      var afterUpdate = await MockInterviewService.GetMockInterviewByIdAsync(mockInterviewId);
      Assert.NotNull(afterUpdate);
      Assert.Equal(newUserId, afterUpdate.IntervieweeUserId);
      Assert.Equal(90, afterUpdate.TimeTakenInMinutes);
    }

    [Fact]
    public async Task Delete_MockInterview_Removes_It()
    {
      var intervieweeEmail = _faker.Internet.Email().ToLower();
      var intervieweeUserId = await _seeder.SeedUserAsync(intervieweeEmail);
      var interviewerEmail = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(interviewerEmail);

      var mockInterviewId = await _seeder.SeedMockInterviewAsync(
        interviewerEmail: interviewerEmail,
        intervieweeUserId: intervieweeUserId
      );

      var request = new DeleteMockInterviewRequest
      {
        MockInterviewId = mockInterviewId,
        Email = interviewerEmail,
      };
      var deleteResp = await MockInterviewService.DeleteMockInterview(request);
      Assert.NotNull(deleteResp);

      var afterDelete = await MockInterviewService.GetMockInterviewByIdAsync(mockInterviewId);
      Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Delete_MockInterview_Fails_If_Wrong_Email()
    {
      var email1 = _faker.Internet.Email().ToLower();
      var intervieweeUserId = await _seeder.SeedUserAsync(email1);
      var email2 = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email2);

      var mockInterviewId = await _seeder.SeedMockInterviewAsync(
        interviewerEmail: email2,
        intervieweeUserId: intervieweeUserId
      );

      var request = new DeleteMockInterviewRequest
      {
        MockInterviewId = mockInterviewId,
        Email = _faker.Internet.Email().ToLower(),
      };
      await Assert.ThrowsAsync<ArgumentException>(
        () => MockInterviewService.DeleteMockInterview(request)
      );
    }

    [Fact]
    public async Task ListMockInterview_Succeeds()
    {
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);
      await _seeder.SeedMockInterviewAsync(email);

      var listRequest = new ListMockInterviewRequest
      {
        Emails = new List<string> { email },
        IncludeBehavioural = true,
        IncludeLeetcode = true,
        IncludeCustom = true,
      };
      var listResp = await MockInterviewService.ListMockInterview(listRequest);
      Assert.NotNull(listResp);
      Assert.NotNull(listResp.MockInterviews);
      Assert.NotEmpty(listResp.MockInterviews);
    }
  }
}
