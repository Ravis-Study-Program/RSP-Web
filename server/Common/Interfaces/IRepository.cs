using System.Linq.Expressions;

namespace RSPWebAPI.Common.Interfaces;

public interface IRepository<TEntity>
  where TEntity : class
{
  IQueryable<TEntity> Table { get; }
  IQueryable<TEntity> TableNoTracking { get; }

  // CRUD Operations
  Task<TEntity?> GetByIdAsync(
    string id,
    CancellationToken cancellationToken = default,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null
  );

  Task<IEnumerable<TEntity>> GetAllAsync(
    Expression<Func<TEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null
  );

  Task<TEntity?> FirstOrDefaultAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null
  );

  /// <summary>
  /// Gets a paginated result with filtering and sorting support
  /// </summary>
  /// <param name="page">Page number (1-based)</param>
  /// <param name="pageSize">Number of items per page</param>
  /// <param name="predicate">Optional filter predicate</param>
  /// <param name="orderBy">Optional ordering function</param>
  /// <param name="include">Optional related data to include</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Tuple of (items, totalCount)</returns>
  Task<(IList<TEntity> Items, int TotalCount)> GetPagedAsync(
    int page,
    int pageSize,
    Expression<Func<TEntity, bool>>? predicate = null,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Gets a cursor-based paginated result for efficient sequential navigation
  /// </summary>
  /// <param name="pageSize">Number of items to return</param>
  /// <param name="predicate">Optional filter predicate</param>
  /// <param name="orderBy">Ordering function (must match cursor fields)</param>
  /// <param name="cursorSelector">Function to extract cursor values (timestamp, id) from entity</param>
  /// <param name="cursor">Optional cursor for pagination (timestamp, id). Null returns first page</param>
  /// <param name="include">Optional related data to include</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Tuple of (items, hasMore, hasPrevious)</returns>
  Task<(IList<TEntity> Items, bool HasMore, bool HasPrevious)> GetPagedWithCursorAsync(
    int pageSize,
    Expression<Func<TEntity, bool>>? predicate,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
    Func<TEntity, (DateTime timestamp, string id)> cursorSelector,
    (DateTime timestamp, string id)? cursor = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
    CancellationToken cancellationToken = default
  );

  Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
  Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

  void Update(TEntity entity, CancellationToken cancellationToken = default);
  void UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

  void Delete(TEntity entity, CancellationToken cancellationToken = default);
  void DeleteRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

  Task<int> DeleteWhereAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default
  );

  // Soft Delete Support
  void Restore(TEntity entity, CancellationToken cancellationToken = default);
  void RestoreRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
