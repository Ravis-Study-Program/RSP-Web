using Bogus;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
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
  [Collection("Database collection")]
  public class CursorPaginationTests : BaseIntegrationTest, IAsyncLifetime
  {
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly TestDataSeeder _seeder;
    private readonly Faker _faker = new();
    private IRepository<MockInterviewEntity> MockInterviewRepository =>
      GetService<IRepository<MockInterviewEntity>>();

    public CursorPaginationTests(IntegrationTestWebAppFactory factory)
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
    public async Task GetPagedWithCursor_Forward_ReturnsCorrectPages()
    {
      // Arrange: Seed 5 mock interviews with delays to ensure distinct timestamps
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);

      var mockInterviewIds = new List<string>();
      for (var i = 0; i < 5; i++)
      {
        var id = await _seeder.SeedMockInterviewAsync(email);
        mockInterviewIds.Add(id);
        await Task.Delay(10); // Small delay to ensure different CreatedAtUtc timestamps
      }

      // Act: Get first page (pageSize = 2)
      var page1Result = await MockInterviewRepository.GetPagedWithCursorAsync(
        pageSize: 2,
        predicate: null,
        orderBy: query =>
          query.OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.MockInterviewId),
        cursorSelector: entity => (entity.CreatedAtUtc, entity.MockInterviewId),
        cursor: null,
        forward: true
      );

      // Assert Page 1
      Assert.Equal(2, page1Result.Items.Count);
      Assert.True(page1Result.HasMore, "Page 1 should have more items");
      Assert.False(page1Result.HasPrevious, "Page 1 should not have previous items");

      // Get the cursor from the last item of page 1
      var lastItemPage1 = page1Result.Items.Last();
      var cursorForPage2 = (lastItemPage1.CreatedAtUtc, lastItemPage1.MockInterviewId);

      // Act: Get second page using cursor
      var page2Result = await MockInterviewRepository.GetPagedWithCursorAsync(
        pageSize: 2,
        predicate: null,
        orderBy: query =>
          query.OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.MockInterviewId),
        cursorSelector: entity => (entity.CreatedAtUtc, entity.MockInterviewId),
        cursor: cursorForPage2,
        forward: true
      );

      // Assert Page 2
      Assert.Equal(2, page2Result.Items.Count);
      Assert.True(page2Result.HasMore, "Page 2 should have more items");
      Assert.True(page2Result.HasPrevious, "Page 2 should have previous items");

      // Verify no overlap between pages
      var page1Ids = page1Result.Items.Select(m => m.MockInterviewId).ToList();
      var page2Ids = page2Result.Items.Select(m => m.MockInterviewId).ToList();
      Assert.Empty(page1Ids.Intersect(page2Ids));

      // Get the cursor from the last item of page 2
      var lastItemPage2 = page2Result.Items.Last();
      var cursorForPage3 = (lastItemPage2.CreatedAtUtc, lastItemPage2.MockInterviewId);

      // Act: Get third page (should have 1 remaining item)
      var page3Result = await MockInterviewRepository.GetPagedWithCursorAsync(
        pageSize: 2,
        predicate: null,
        orderBy: query =>
          query.OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.MockInterviewId),
        cursorSelector: entity => (entity.CreatedAtUtc, entity.MockInterviewId),
        cursor: cursorForPage3,
        forward: true
      );

      // Assert Page 3
      Assert.Single(page3Result.Items);
      Assert.False(page3Result.HasMore, "Page 3 should not have more items");
      Assert.True(page3Result.HasPrevious, "Page 3 should have previous items");

      // Verify all 5 items were retrieved across all pages
      var allRetrievedIds = page1Ids.Concat(page2Ids).Concat(page3Result.Items.Select(m => m.MockInterviewId)).ToList();
      Assert.Equal(5, allRetrievedIds.Count);
      Assert.Equal(5, allRetrievedIds.Distinct().Count());
    }

    [Fact]
    public async Task GetPagedWithCursor_Backward_ReturnsPreviousPage()
    {
      // Arrange: Seed 5 mock interviews with delays to ensure distinct timestamps
      var email = _faker.Internet.Email().ToLower();
      await _seeder.SeedUserAsync(email);

      var mockInterviewIds = new List<string>();
      for (var i = 0; i < 5; i++)
      {
        var id = await _seeder.SeedMockInterviewAsync(email);
        mockInterviewIds.Add(id);
        await Task.Delay(10); // Small delay to ensure different CreatedAtUtc timestamps
      }

      // Navigate forward to page 2 first
      var page1Result = await MockInterviewRepository.GetPagedWithCursorAsync(
        pageSize: 2,
        predicate: null,
        orderBy: query =>
          query.OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.MockInterviewId),
        cursorSelector: entity => (entity.CreatedAtUtc, entity.MockInterviewId),
        cursor: null,
        forward: true
      );

      var lastItemPage1 = page1Result.Items.Last();
      var cursorForPage2 = (lastItemPage1.CreatedAtUtc, lastItemPage1.MockInterviewId);

      var page2Result = await MockInterviewRepository.GetPagedWithCursorAsync(
        pageSize: 2,
        predicate: null,
        orderBy: query =>
          query.OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.MockInterviewId),
        cursorSelector: entity => (entity.CreatedAtUtc, entity.MockInterviewId),
        cursor: cursorForPage2,
        forward: true
      );

      // Get the cursor from the first item of page 2 for backward navigation
      var firstItemPage2 = page2Result.Items.First();
      var cursorForBackward = (firstItemPage2.CreatedAtUtc, firstItemPage2.MockInterviewId);

      // Act: Navigate backward from page 2
      var backwardResult = await MockInterviewRepository.GetPagedWithCursorAsync(
        pageSize: 2,
        predicate: null,
        orderBy: query =>
          query.OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.MockInterviewId),
        cursorSelector: entity => (entity.CreatedAtUtc, entity.MockInterviewId),
        cursor: cursorForBackward,
        forward: false
      );

      // Assert: Backward navigation returns page 1 items in correct DESC order
      Assert.Equal(2, backwardResult.Items.Count);
      Assert.True(backwardResult.HasPrevious, "Backward result should have previous indicator");

      // Verify the items match page 1 (should be the same items in the same order)
      var page1Ids = page1Result.Items.Select(m => m.MockInterviewId).ToList();
      var backwardIds = backwardResult.Items.Select(m => m.MockInterviewId).ToList();
      Assert.Equal(page1Ids, backwardIds);

      // Verify DESC ordering is maintained (newer items first)
      for (var i = 0; i < backwardResult.Items.Count - 1; i++)
      {
        var currentItem = backwardResult.Items[i];
        var nextItem = backwardResult.Items[i + 1];

        var currentTimestamp = currentItem.CreatedAtUtc;
        var nextTimestamp = nextItem.CreatedAtUtc;

        // Current item should be newer (greater timestamp) or same timestamp with greater ID
        Assert.True(
          currentTimestamp > nextTimestamp ||
          (currentTimestamp == nextTimestamp &&
           string.Compare(currentItem.MockInterviewId, nextItem.MockInterviewId) > 0),
          "Items should be in DESC order (newest first)"
        );
      }
    }
  }
}
