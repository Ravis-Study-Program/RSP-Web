using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using RSPWebAPI.Common.Cache;

namespace RSPWebAPI.Tests.Shared;

public class DummyRequestCache : IRequestCache
{
  public async Task<T?> GetOrCreateAsync<T>(
    string routeKey,
    string? primaryKey,
    Func<Task<T>> factory,
    TimeSpan? ttl = null
  )
  {
    return await factory();
  }

  public async Task<T?> GetOrCreateAsync<T>(
    string routeKey,
    Func<Task<T>> factory,
    TimeSpan? ttl = null
  )
  {
    return await factory();
  }

  public void Remove(string routeKey, string? primaryKey) { }
}
