using System;
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
        Email = email,
        IncludeBehavioural = false,
        IncludeLeetcode = false,
        IncludeCustom = false,
      };
      var listResponse = await MockInterviewService.ListMockInterview(listRequest);
      Assert.Equal(Message.MockInterviewListSuccessfully, listResponse.Message);
      Assert.True(listResponse.IsSuccess);
      Assert.NotNull(listResponse.Data?.MockInterviews);
      Assert.Equal(count, listResponse.Data.MockInterviews.Count);
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
      await _seeder.SeedUserAsync(email2);
      var enrollmentId = await _seeder.SeedEnrollmentAsync(userId: user1Id);

      var request = new CreateMockInterviewRequest
      {
        IntervieweeEmail = email2,
        InterviewerUserId = user1Id,
        EnrollmentId = enrollmentId,
        StartDate = DateTime.UtcNow,
      };
      var response = await MockInterviewService.CreateMockInterview(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.EnrollmentDoesNotExists, response.Message);
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
        EnrollmentId = existing.EnrollmentId,
        InterviewerUserId = newUserId,
        IntervieweeEmail = newUserEmail,
        StartDate = DateTime.UtcNow.AddDays(2),
        TimeTakenInMinutes = 90,
        MockInterviewRounds = new(),
      };

      var updateResp = await MockInterviewService.UpdateMockInterview(updateRequest);
      Assert.True(updateResp.IsSuccess);
      Assert.Equal(Message.MockInterviewUpdatedSuccessfully, updateResp.Message);

      var afterUpdate = await MockInterviewService.GetMockInterviewByIdAsync(mockInterviewId);
      Assert.NotNull(afterUpdate);
      Assert.Equal(newUserId, afterUpdate!.InterviewerUserId);
      Assert.Equal(90, afterUpdate.TimeTakenInMinutes);
    }

    [Fact]
    public async Task Delete_MockInterview_Removes_It()
    {
      var interviewerEmail = _faker.Internet.Email().ToLower();
      var interviewerId = await _seeder.SeedUserAsync(interviewerEmail);
      var intervieweeEmail = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(intervieweeEmail);

      var mockInterviewId = await _seeder.SeedMockInterviewAsync(
        intervieweeEmail,
        interviewerUserId: interviewerId
      );

      var request = new DeleteMockInterviewRequest
      {
        MockInterviewId = mockInterviewId,
        Email = interviewerEmail,
      };
      var deleteResp = await MockInterviewService.DeleteMockInterview(request);
      Assert.True(deleteResp.IsSuccess);
      Assert.Equal(Message.MockInterviewDeletedSuccessfully, deleteResp.Message);

      var afterDelete = await MockInterviewService.GetMockInterviewByIdAsync(mockInterviewId);
      Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Delete_MockInterview_Fails_If_Wrong_Email()
    {
      var email1 = _faker.Internet.Email().ToLower();
      var interviewerId = await _seeder.SeedUserAsync(email1);
      var email2 = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email2);

      var mockInterviewId = await _seeder.SeedMockInterviewAsync(
        email2,
        interviewerUserId: interviewerId
      );

      var request = new DeleteMockInterviewRequest
      {
        MockInterviewId = mockInterviewId,
        Email = _faker.Internet.Email().ToLower(),
      };
      var deleteResp = await MockInterviewService.DeleteMockInterview(request);
      Assert.False(deleteResp.IsSuccess);
      Assert.Equal(Message.MockInterviewDoesNotExists, deleteResp.Message);
    }

    [Fact]
    public async Task ListMockInterview_Succeeds()
    {
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);
      await _seeder.SeedMockInterviewAsync(email);

      var listRequest = new ListMockInterviewRequest
      {
        Email = email,
        IncludeBehavioural = true,
        IncludeLeetcode = true,
        IncludeCustom = true,
      };
      var listResp = await MockInterviewService.ListMockInterview(listRequest);
      Assert.True(listResp.IsSuccess);
      Assert.Equal(Message.MockInterviewListSuccessfully, listResp.Message);
      Assert.NotNull(listResp.Data?.MockInterviews);
      Assert.NotEmpty(listResp.Data.MockInterviews);
    }
  }
}
