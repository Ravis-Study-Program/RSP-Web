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
