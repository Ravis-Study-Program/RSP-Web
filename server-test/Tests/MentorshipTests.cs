using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.Mentorships;
using RSPWebAPI.Features.Mentorships.Dtos;
using RSPWebAPI.Features.Mentorships.Interfaces;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Seasons;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Tests
{
  public class MentorshipTests : BaseIntegrationTest, IAsyncLifetime
  {
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly TestDataSeeder _seeder;
    private readonly Faker _faker = new();
    private IMentorshipService MentorshipService => GetService<IMentorshipService>();
    private ISeasonService SeasonService => GetService<ISeasonService>();
    private IUserService UserService => GetService<IUserService>();

    public MentorshipTests(IntegrationTestWebAppFactory factory)
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
    public async Task Create_And_List_Mentorships()
    {
      const int count = 2;
      for (var i = 0; i < count; i++)
      {
        await _seeder.SeedMentorshipAsync();
      }

      var listRequest = new AdminListMentorshipRequest();
      var listResponse = await MentorshipService.ListAdminMentorship(listRequest);
      Assert.NotNull(listResponse.Mentorships);

      var all = await _seeder.GetAllMentorshipsAsync();
      Assert.Equal(count, all.Count);
    }

    [Fact]
    public async Task CreateMentorship_Succeeds()
    {
      var mentorshipId = await _seeder.SeedMentorshipAsync();
      var all = await _seeder.GetAllMentorshipsAsync();
      Assert.Single(all);
      Assert.Equal(mentorshipId, all.First().MentorshipId);
    }

    [Fact]
    public async Task CreateMentorship_Fails_If_MentorNotFound()
    {
      var menteeEnrollmentId = await _seeder.SeedEnrollmentAsync(role: SeasonRole.Student);

      var request = new AdminCreateMentorshipRequest
      {
        MentorEnrollmentId = _faker.Random.AlphaNumeric(12),
        MenteeEnrollmentId = menteeEnrollmentId,
      };
      await Assert.ThrowsAsync<KeyNotFoundException>(
        () => MentorshipService.CreateAdminMentorship(request)
      );
    }

    [Fact]
    public async Task CreateMentorship_Fails_If_MenteeNotFound()
    {
      var mentorEnrollmentId = await _seeder.SeedEnrollmentAsync(
        role: SeasonRole.Mentor,
        rolePromotion: SeasonStudentRolePromotion.NotApplicable
      );

      var request = new AdminCreateMentorshipRequest
      {
        MentorEnrollmentId = mentorEnrollmentId,
        MenteeEnrollmentId = _faker.Random.AlphaNumeric(
          12
        ) // invalid
        ,
      };

      await Assert.ThrowsAsync<KeyNotFoundException>(
        () => MentorshipService.CreateAdminMentorship(request)
      );
    }

    [Fact]
    public async Task CreateMentorship_Fails_If_AlreadyExists()
    {
      await _seeder.SeedMentorshipAsync();
      var mentorship = await _seeder.GetAllMentorshipsAsync();
      var pair = mentorship.First();

      var request = new AdminCreateMentorshipRequest
      {
        MentorEnrollmentId = pair.MentorEnrollmentId,
        MenteeEnrollmentId = pair.MenteeEnrollmentId,
      };
      await Assert.ThrowsAsync<InvalidOperationException>(
        () => MentorshipService.CreateAdminMentorship(request)
      );
    }

    [Fact]
    public async Task Update_Mentorship_Should_Reflect_New_Values()
    {
      var mentorshipId = await _seeder.SeedMentorshipAsync();
      var existing = await MentorshipService.GetMentorshipByIdAsync(mentorshipId);
      Assert.NotNull(existing);

      var seasonId = await _seeder.SeedSeasonAndSeasonWeeks();
      var newMentorId = await _seeder.SeedEnrollmentAsync(
        seasonId: seasonId,
        role: SeasonRole.Mentor,
        rolePromotion: SeasonStudentRolePromotion.NotApplicable
      );
      var newMenteeId = await _seeder.SeedEnrollmentAsync(
        seasonId: seasonId,
        role: SeasonRole.Student
      );

      var updateRequest = new AdminUpdateMentorshipRequest
      {
        MentorshipId = mentorshipId,
        MentorEnrollmentId = newMentorId,
        MenteeEnrollmentId = newMenteeId,
      };

      var updateResp = await MentorshipService.UpdateAdminMentorship(updateRequest);

      var afterUpdate = await MentorshipService.GetMentorshipByIdAsync(mentorshipId);
      Assert.NotNull(afterUpdate);
      Assert.Equal(newMentorId, afterUpdate!.MentorEnrollmentId);
      Assert.Equal(newMenteeId, afterUpdate.MenteeEnrollmentId);
    }

    [Fact]
    public async Task Update_Mentorship_Fails_If_Mentorship_NotFound()
    {
      var request = new AdminUpdateMentorshipRequest
      {
        MentorshipId = _faker.Random.AlphaNumeric(10),
        MentorEnrollmentId = _faker.Random.AlphaNumeric(10),
        MenteeEnrollmentId = _faker.Random.AlphaNumeric(10),
      };
      await Assert.ThrowsAsync<KeyNotFoundException>(
        () => MentorshipService.UpdateAdminMentorship(request)
      );
    }

    [Fact]
    public async Task Delete_Mentorship_Removes_It()
    {
      var mentorshipId = await _seeder.SeedMentorshipAsync();
      var all = await _seeder.GetAllMentorshipsAsync();
      Assert.Single(all);

      var deleteRequest = new AdminDeleteMentorshipRequest { MentorshipId = mentorshipId };
      var deleteResp = await MentorshipService.DeleteAdminMentorship(deleteRequest);

      var afterDelete = await MentorshipService.GetMentorshipByIdAsync(mentorshipId);
      Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Delete_Mentorship_Fails_If_NotFound()
    {
      var request = new AdminDeleteMentorshipRequest
      {
        MentorshipId = _faker.Random.AlphaNumeric(10),
      };
      await Assert.ThrowsAsync<KeyNotFoundException>(
        () => MentorshipService.DeleteAdminMentorship(request)
      );
    }

    [Fact]
    public async Task ListAdminMentorship_Returns_Success()
    {
      var request = new AdminListMentorshipRequest();
      var listResp = await MentorshipService.ListAdminMentorship(request);
      Assert.NotNull(listResp.Mentorships);
    }

    [Fact]
    public async Task GetCurrentUserMenteesList_Returns_Mentees()
    {
      var seasonId = await _seeder.SeedSeasonAndSeasonWeeks();
      var mentorUserId = await _seeder.SeedUserAsync();
      var mentorEnrollmentId = await _seeder.SeedEnrollmentAsync(
        seasonId: seasonId,
        userId: mentorUserId,
        role: SeasonRole.Mentor,
        rolePromotion: SeasonStudentRolePromotion.NotApplicable
      );
      var menteeId = await _seeder.SeedEnrollmentAsync(
        seasonId: seasonId,
        role: SeasonRole.Student
      );

      await _seeder.SeedMentorshipAsync(mentorEnrollmentId, menteeId);

      var season = await SeasonService.GetSeasonByIdAsync(seasonId);
      Assert.NotNull(season);
      var mentorUser = await UserService.GetUserAsync(userId: mentorUserId);
      Assert.NotNull(mentorUser);

      var menteesReq = new GetCurrentUserMenteesListRequest
      {
        UserId = mentorUser.UserId,
        SeasonSlug = season.Slug,
      };
      var menteesResp = await MentorshipService.GetCurrentUserMenteesList(menteesReq);
      Assert.Single(menteesResp.Mentorships);
    }
  }
}
