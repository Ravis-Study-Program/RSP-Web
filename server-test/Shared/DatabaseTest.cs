using AutoFixture;
using RSPWebAPI.Database;
using Xunit;

namespace RSPWebAPI.Tests.Shared;

[Collection(nameof(DatabaseTestCollection))]
public abstract class DatabaseTest : IAsyncLifetime
{
  private Func<Task> _resetDatabase;
  protected readonly ApplicationDbContext Db;
  protected readonly Fixture Fixture;

  public DatabaseTest(IntegrationTestWebAppFactory factory)
  {
    _resetDatabase = factory.ResetDatabase;
    Db = factory.Db;
    Fixture = new Fixture();
    // Fixture.Customize(new NoCircularReferencesCustomization());
    // Fixture.Customize(new IgnoreVirtualMembersCustomization());
  }

  public async Task Insert<T>(T entity)
    where T : class
  {
    await Db.AddAsync(entity);
    await Db.SaveChangesAsync();
  }

  public Task InitializeAsync() => Task.CompletedTask;

  public Task DisposeAsync() => _resetDatabase();
}
