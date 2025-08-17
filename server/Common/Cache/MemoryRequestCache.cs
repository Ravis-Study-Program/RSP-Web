using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Prometheus;

namespace RSPWebAPI.Common.Cache;

public class MemoryRequestCache : IRequestCache
{
  private static readonly Counter CacheStatus = Metrics.CreateCounter(
    "cache_status_total",
    "Cache hit vs miss",
    new CounterConfiguration { LabelNames = new[] { "route", "cached" } }
  );

  private static readonly Histogram RequestDuration = Metrics.CreateHistogram(
    "request_duration_seconds",
    "Request latency by route & cache status",
    new HistogramConfiguration { LabelNames = new[] { "route", "cached" } }
  );

  private readonly IMemoryCache _cache;
  private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(5);

  public MemoryRequestCache(IMemoryCache cache)
  {
    _cache = cache;
  }

  public async Task<T?> GetOrCreateAsync<T>(
    string routeKey,
    string? primaryKey,
    Func<Task<T>> factory,
    TimeSpan? ttl = null
  )
  {
    var sw = Stopwatch.StartNew();
    var cacheKey = string.IsNullOrEmpty(primaryKey) ? routeKey : $"{routeKey}:{primaryKey}";

    if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is T cached)
    {
      CacheStatus.WithLabels(routeKey, "hit").Inc();
      RequestDuration.WithLabels(routeKey, "hit").Observe(sw.Elapsed.TotalSeconds);
      return cached;
    }

    CacheStatus.WithLabels(routeKey, "miss").Inc();
    var fresh = await factory();
    RequestDuration.WithLabels(routeKey, "miss").Observe(sw.Elapsed.TotalSeconds);
    if (fresh is null)
    {
      return default;
    }

    _cache.Set(cacheKey, fresh, ttl ?? _defaultTtl);
    return fresh;
  }

  public Task<T?> GetOrCreateAsync<T>(
    string routeKey,
    Func<Task<T>> factory,
    TimeSpan? ttl = null
  ) => GetOrCreateAsync(routeKey, primaryKey: null, factory, ttl);

  public void Remove(string routeKey, string? primaryKey = null)
  {
    var key = string.IsNullOrEmpty(primaryKey) ? routeKey : $"{routeKey}:{primaryKey}";
    _cache.Remove(key);
  }
}
