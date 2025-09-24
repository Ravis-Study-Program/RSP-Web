using System.Data.Common;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Respawn;
using RSPWebAPI.Clients.Interfaces;
using RSPWebAPI.Common.Cache;
using RSPWebAPI.Database;
using RSPWebAPI.Features.Enrollments;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.LeetcodeProblemRecommendations;
using RSPWebAPI.Features.LeetcodeProblemRecommendations.Interfaces;
using RSPWebAPI.Features.Leetcodes;
using RSPWebAPI.Features.Leetcodes.Interfaces;
using RSPWebAPI.Features.Mentorships;
using RSPWebAPI.Features.Mentorships.Interfaces;
using RSPWebAPI.Features.MockInterviews;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Features.ProblemAttempts;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Seasons;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Tests.Shared;
using Testcontainers.PostgreSql;
using Xunit;

namespace RSPWebAPI.Tests;

[CollectionDefinition(nameof(DatabaseTestCollection))]
public class DatabaseTestCollection : ICollectionFixture<IntegrationTestWebAppFactory> { }

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
    .WithImage("postgres:latest") // You may want to change this to be the version your production db is on
    .WithDatabase("db")
    .WithUsername("postgres")
    .WithPassword("postgres")
    .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
    .WithCleanUp(true)
    .Build();

  public ApplicationDbContext Db { get; private set; } = null!;
  private Respawner _respawner = null!;
  private DbConnection _connection = null!;

  public Mock<IUserIdentityService> MockUserIdentityService { get; } = new();

  public async Task ResetDatabase()
  {
    await _respawner.ResetAsync(_connection);
  }

  public async Task InitializeAsync()
  {
    await _container.StartAsync();

    Db = Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await Db.Database.MigrateAsync();

    _connection = Db.Database.GetDbConnection();
    await _connection.OpenAsync();

    _respawner = await Respawner.CreateAsync(
      _connection,
      new RespawnerOptions { DbAdapter = DbAdapter.Postgres, SchemasToInclude = new[] { "public" } }
    );
  }

  public new async Task DisposeAsync()
  {
    await _connection.CloseAsync();
    await _container.DisposeAsync();
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureTestServices(services =>
    {
      services.RemoveDbContext<ApplicationDbContext>();
      services.AddDbContext<ApplicationDbContext>(options =>
      {
        options.UseNpgsql(_container.GetConnectionString());
      });

      // Mocks
      var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserIdentityService));
      if (descriptor is not null)
      {
        services.Remove(descriptor);
      }
      services.AddSingleton(MockUserIdentityService.Object);

      var memCacheDescriptor = services.SingleOrDefault(d =>
        d.ServiceType == typeof(IRequestCache)
      );
      if (memCacheDescriptor != null)
        services.Remove(memCacheDescriptor);

      // 2) Add your no-op cache
      services.AddSingleton<IRequestCache, DummyRequestCache>();
    });

    // Set any necessary environment variables.
    Environment.SetEnvironmentVariable("PGHOST", "localhost");
    Environment.SetEnvironmentVariable("PGPORT", "5432");
    Environment.SetEnvironmentVariable("PGDATABASE", "testdb");
    Environment.SetEnvironmentVariable("PGUSER", "testuser");
    Environment.SetEnvironmentVariable("PGPASSWORD", "testpassword");
    Environment.SetEnvironmentVariable("AUTH0_DOMAIN", "https://test.auth0.com");
    Environment.SetEnvironmentVariable("AUTH0_AUDIENCE", "https://api.test.com");
    Environment.SetEnvironmentVariable("AUTH0_CLIENT_ID", "super-secret-auth0-client-id");
    Environment.SetEnvironmentVariable("AUTH0_CLIENT_SECRET", "super-secret-auth0-client-secret");
    Environment.SetEnvironmentVariable("PORT", "4000");
    Environment.SetEnvironmentVariable("ALLOWED_ORIGINS", "http://localhost:3000");
    Environment.SetEnvironmentVariable(
      "AUTH0_MANAGEMENT_API_DOMAIN",
      "http://managementapi.auth0.com"
    );
    Environment.SetEnvironmentVariable("AUTH0_CUSTOM_DOMAIN", "http://customdomain.auth0.com");
  }
}

public static class ServiceCollectionExtensions
{
  public static void RemoveDbContext<T>(this IServiceCollection services)
    where T : DbContext
  {
    var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<T>));
    if (descriptor != null)
    {
      services.Remove(descriptor);
    }
  }

  public static void EnsureDbCreated<T>(this IServiceCollection services)
    where T : DbContext
  {
    using var scope = services.BuildServiceProvider().CreateScope();
    var serviceProvider = scope.ServiceProvider;
    var context = serviceProvider.GetRequiredService<T>();
    context.Database.EnsureCreated();
  }
}
