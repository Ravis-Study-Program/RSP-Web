using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RSPWebAPI.Database;
using Xunit;

namespace RSPWebAPI.Tests.Shared;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
  private readonly IServiceScope _scope;
  protected readonly ApplicationDbContext DbContext;

  protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
  {
    _scope = factory.Services.CreateScope();
    DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  }

  protected T GetService<T>()
    where T : notnull
  {
    return _scope.ServiceProvider.GetRequiredService<T>();
  }

  public void Dispose()
  {
    _scope?.Dispose();
  }
}
