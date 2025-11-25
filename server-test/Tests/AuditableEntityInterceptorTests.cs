using Bogus;
using RSPWebAPI.Entities;
using RSPWebAPI.Database;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Tests;

[Collection(nameof(DatabaseTestCollection))]
public class AuditableEntityInterceptorTests : BaseIntegrationTest, IAsyncLifetime
{
  private readonly IntegrationTestWebAppFactory _factory;
  private readonly Faker _faker = new();

  public AuditableEntityInterceptorTests(IntegrationTestWebAppFactory factory)
    : base(factory)
  {
    _factory = factory;
  }

  public async Task InitializeAsync()
  {
    await _factory.ResetDatabase();
  }

  public Task DisposeAsync() => Task.CompletedTask;

  [Fact]
  public async Task Create_SetsCreatedAtUtcAndUpdatedAtUtc_ToSameValue()
  {
    var user = new UserEntity
    {
      UserId = Constants.GeneratePrimaryKeyId(),
      Email = _faker.Internet.Email().ToLower(),
      Name = _faker.Name.FullName(),
      Slug = _faker.Random.AlphaNumeric(8),
      IsAdmin = false,
      IsTestUser = false,
    };

    await DbContext.Users.AddAsync(user);
    await DbContext.SaveChangesAsync();

    Assert.NotEqual(default, user.CreatedAtUtc);
    Assert.NotEqual(default, user.UpdatedAtUtc);
    Assert.Equal(user.CreatedAtUtc, user.UpdatedAtUtc);
  }

  [Fact]
  public async Task Update_UpdatesOnlyUpdatedAtUtc_LeavesCreatedAtUtcUnchanged()
  {
    var user = new UserEntity
    {
      UserId = Constants.GeneratePrimaryKeyId(),
      Email = _faker.Internet.Email().ToLower(),
      Name = _faker.Name.FullName(),
      Slug = _faker.Random.AlphaNumeric(8),
      IsAdmin = false,
      IsTestUser = false,
    };

    await DbContext.Users.AddAsync(user);
    await DbContext.SaveChangesAsync();

    var originalCreatedAtUtc = user.CreatedAtUtc;
    var originalUpdatedAtUtc = user.UpdatedAtUtc;

    await Task.Delay(10);
    user.Name = _faker.Name.FullName();
    await DbContext.SaveChangesAsync();

    Assert.Equal(originalCreatedAtUtc, user.CreatedAtUtc);
    Assert.True(user.UpdatedAtUtc > originalUpdatedAtUtc);
  }
}
