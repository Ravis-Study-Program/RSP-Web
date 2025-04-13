using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments;
using RSPWebAPI.Features.Enrollments.Dtos;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.Mentorships.Interfaces;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Tests
{
  public class EnrollmentTests : BaseIntegrationTest, IAsyncLifetime
  {
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly TestDataSeeder _seeder;
    private readonly Faker _faker = new();
    private IEnrollmentService EnrollmentService => GetService<IEnrollmentService>();
    private ISeasonService SeasonService => GetService<ISeasonService>();

    public EnrollmentTests(IntegrationTestWebAppFactory factory)
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
    public async Task Create_And_List_Enrollments()
    {
      const int count = 2;
      for (var i = 0; i < count; i++)
      {
        var userId = await _seeder.SeedUserAsync();
        var seasonId = await _seeder.SeedSeasonAsync();
        await _seeder.SeedEnrollmentAsync(seasonId, userId);
      }

      var listRequest = new AdminListEnrollmentRequest();
      var listResponse = await EnrollmentService.ListAdminEnrollment(listRequest);
      Assert.True(listResponse.IsSuccess);
      Assert.Equal(Message.EnrollmentListSuccessfully, listResponse.Message);

      var all = await _seeder.GetAllEnrollmentsAsync();
      Assert.Equal(count, all.Count);
    }

    [Fact]
    public async Task Create_Enrollment_Fails_If_Already_Exists()
    {
      var userId = await _seeder.SeedUserAsync();
      var seasonId = await _seeder.SeedSeasonAsync();
      await _seeder.SeedEnrollmentAsync(seasonId, userId);

      var request = new AdminCreateEnrollmentRequest
      {
        SeasonId = seasonId,
        UserId = userId,
        Role = SeasonRole.Student,
        StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
      };
      var response = await EnrollmentService.CreateAdminEnrollment(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.EnrollmentExists, response.Message);
    }

    [Fact]
    public async Task Update_Enrollment_Should_Reflect_New_Values()
    {
      var enrollmentId = await _seeder.SeedEnrollmentAsync();

      var newUserId = await _seeder.SeedUserAsync();
      var newSeasonId = await _seeder.SeedSeasonAsync();

      var updateRequest = new AdminUpdateEnrollmentRequest
      {
        EnrollmentId = enrollmentId,
        SeasonId = newSeasonId,
        UserId = newUserId,
        Role = SeasonRole.Mentor,
        StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
      };
      var updateResponse = await EnrollmentService.UpdateAdminEnrollment(updateRequest);
      Assert.True(updateResponse.IsSuccess);
      Assert.Equal(Message.EnrollmentUpdatedSuccessfully, updateResponse.Message);

      var updated = await EnrollmentService.GetEnrollmentByIdAsync(enrollmentId);
      Assert.NotNull(updated);
      Assert.Equal(updateRequest.SeasonId, updated!.SeasonId);
      Assert.Equal(updateRequest.UserId, updated.UserId);
      Assert.Equal(updateRequest.Role, updated.Role);
      Assert.Equal(updateRequest.StudentRolePromotion, updated.StudentRolePromotion);
    }

    [Fact]
    public async Task Delete_Enrollment_Removes_It()
    {
      var userId = await _seeder.SeedUserAsync();
      var seasonId = await _seeder.SeedSeasonAsync();
      var enrollmentId = await _seeder.SeedEnrollmentAsync(seasonId, userId);

      var request = new AdminDeleteEnrollmentRequest { EnrollmentId = enrollmentId };
      var deleteResponse = await EnrollmentService.DeleteAdminEnrollment(request);
      Assert.True(deleteResponse.IsSuccess);
      Assert.Equal(Message.EnrollmentDeletedSuccessfully, deleteResponse.Message);

      var afterDelete = await EnrollmentService.GetEnrollmentByIdAsync(enrollmentId);
      Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Delete_Enrollment_Fails_If_Not_Found()
    {
      var request = new AdminDeleteEnrollmentRequest
      {
        EnrollmentId = _faker.Random.AlphaNumeric(12),
      };
      var response = await EnrollmentService.DeleteAdminEnrollment(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.EnrollmentDoesNotExists, response.Message);
    }

    [Fact]
    public async Task ListAdminEnrollment_Should_Return_Success()
    {
      var request = new AdminListEnrollmentRequest();
      var response = await EnrollmentService.ListAdminEnrollment(request);
      Assert.True(response.IsSuccess);
      Assert.Equal(Message.EnrollmentListSuccessfully, response.Message);
    }

    [Fact]
    public async Task GetUserEnrollments_ReturnsData()
    {
      var email = _faker.Internet.Email().ToLower();
      var userId = await _seeder.SeedUserAsync(email);
      var seasonId = await _seeder.SeedSeasonAsync();
      await _seeder.SeedEnrollmentAsync(seasonId, userId);

      var request = new GetUserEnrollmentsRequest { Email = email };
      var enrollResp = await EnrollmentService.GetUserEnrollments(request);
      Assert.True(enrollResp.IsSuccess);
      Assert.Equal(Message.EnrollmentUsersListSuccessfully, enrollResp.Message);
      Assert.NotEmpty(enrollResp.Data!.Enrollments);
    }

    [Fact]
    public async Task GetEnrollmentUsers_ReturnsData()
    {
      var userId = await _seeder.SeedUserAsync();
      var seasonId = await _seeder.SeedSeasonAsync();
      var season = await SeasonService.GetSeasonByIdAsync(seasonId);
      Assert.NotNull(season);
      await _seeder.SeedEnrollmentAsync(seasonId, userId);

      var request = new GetEnrollmentUsersRequest { SeasonSlug = season.Slug };
      var userResp = await EnrollmentService.GetEnrollmentUsers(request);
      Assert.True(userResp.IsSuccess);
      Assert.Equal(Message.EnrollmentUsersListSuccessfully, userResp.Message);
      Assert.NotEmpty(userResp.Data!.EnrollmentUsers);
    }

    [Fact]
    public async Task GetIsUserEnrolled_Reports_NotEnrolled()
    {
      var request = new GetIsUserEnrolledRequest
      {
        SeasonSlug = _faker.Random.Word(),
        Email = _faker.Internet.Email().ToLower(),
      };
      var response = await EnrollmentService.GetIsUserEnrolled(request);
      Assert.True(response.IsSuccess);
      Assert.False(response.Data!.IsEnrolled);
      Assert.Equal(Message.EnrollmentDoesNotExists, response.Message);
    }

    [Fact]
    public async Task GetIsUserEnrolled_Reports_Enrolled()
    {
      var email = _faker.Internet.Email().ToLower();
      var userId = await _seeder.SeedUserAsync(email);

      var slug = _faker.Random.String2(8);
      var seasonId = await _seeder.SeedSeasonAsync(slug);
      await _seeder.SeedEnrollmentAsync(seasonId, userId);

      var request = new GetIsUserEnrolledRequest { SeasonSlug = slug, Email = email };
      var response = await EnrollmentService.GetIsUserEnrolled(request);
      Assert.True(response.IsSuccess);
      Assert.True(response.Data!.IsEnrolled);
      Assert.Equal(Message.EnrollmentExists, response.Message);
    }

    [Fact]
    public async Task KickStudent_Fails_If_NotEnrolled()
    {
      var request = new KickStudentRequest
      {
        SeasonSlug = _faker.Random.Word(),
        Email = _faker.Internet.Email().ToLower(),
        MenteeEnrollmentId = _faker.Random.String2(8),
      };
      var response = await EnrollmentService.KickStudent(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.EnrollmentDoesNotExists, response.Message);
    }

    [Fact]
    public async Task UpdateStudentRolePromotion_Fails_If_Enrollment_NotFound()
    {
      var request = new UpdateStudentRolePromotionRequest
      {
        SeasonSlug = _faker.Random.Word(),
        Email = _faker.Internet.Email().ToLower(),
        MenteeEnrollmentId = _faker.Random.String2(8),
        StudentRolePromotion = SeasonStudentRolePromotion.Novice,
      };
      var response = await EnrollmentService.UpdateStudentRolePromotion(request);
      Assert.False(response.IsSuccess);
      Assert.Equal(Message.EnrollmentDoesNotExists, response.Message);
    }
  }
}
