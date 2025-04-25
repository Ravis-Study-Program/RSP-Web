namespace RSPWebAPI.Common.Cache;

public interface IRequestCache
{
  Task<T> GetOrCreateAsync<T>(string routeKey, Func<Task<T>> factory, TimeSpan? ttl = null);

  Task<T> GetOrCreateAsync<T>(
    string routeKey,
    string? primaryKey,
    Func<Task<T>> factory,
    TimeSpan? ttl = null
  );

  void Remove(string routeKey, string? primaryKey = null);
}
