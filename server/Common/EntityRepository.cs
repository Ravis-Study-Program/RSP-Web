using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Common;

public class EntityRepository<TEntity> : IRepository<TEntity>
  where TEntity : class
{
  private readonly DbContext _context;
  private readonly DbSet<TEntity> _dbSet;

  public EntityRepository(DbContext context)
  {
    _context = context;
    _dbSet = context.Set<TEntity>();
  }

  public async Task<TEntity?> GetByIdAsync(
    string id,
    CancellationToken cancellationToken = default,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null
  )
  {
    IQueryable<TEntity> query = Table;
    if (include != null)
    {
      query = include(query);
    }

    var keyProperty = _dbSet.EntityType.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
    if (keyProperty == null)
    {
      throw new InvalidOperationException("No primary key defined for the entity.");
    }

    return await query
      .AsTracking()
      .FirstOrDefaultAsync(
        entity => EF.Property<string>(entity, keyProperty) == id,
        cancellationToken
      );
  }

  public async Task<IEnumerable<TEntity>> GetAllAsync(
    Expression<Func<TEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null
  )
  {
    IQueryable<TEntity> query = Table;

    if (include != null)
    {
      query = include(query);
    }

    return predicate == null
      ? await query.ToListAsync(cancellationToken)
      : await query.Where(predicate).ToListAsync(cancellationToken);
  }

  public async Task<TEntity?> FirstOrDefaultAsync(
    Expression<Func<TEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null
  )
  {
    IQueryable<TEntity> query = Table;

    if (include != null)
    {
      query = include(query);
    }

    return predicate == null
      ? await query.FirstOrDefaultAsync(cancellationToken)
      : await query.Where(predicate).FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<(IList<TEntity> Items, int TotalCount)> GetPagedAsync(
    int page,
    int pageSize,
    Expression<Func<TEntity, bool>>? predicate = null,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
    CancellationToken cancellationToken = default
  )
  {
    IQueryable<TEntity> query = TableNoTracking;

    // Apply includes for related data
    if (include != null)
    {
      query = include(query);
    }

    // Apply filtering
    if (predicate != null)
    {
      query = query.Where(predicate);
    }

    // Get total count BEFORE pagination
    var totalCount = await query.CountAsync(cancellationToken);

    // Apply sorting
    if (orderBy != null)
    {
      query = orderBy(query);
    }

    // Apply pagination
    var items = await query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return (items, totalCount);
  }

  public async Task<(IList<TEntity> Items, bool HasMore, bool HasPrevious)> GetPagedWithCursorAsync(
    int pageSize,
    Expression<Func<TEntity, bool>>? predicate,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
    Func<TEntity, (DateTime timestamp, string id)> cursorSelector,
    (DateTime timestamp, string id)? cursor = null,
    bool forward = true,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(orderBy);
    ArgumentNullException.ThrowIfNull(cursorSelector);

    IQueryable<TEntity> query = TableNoTracking;

    // Apply includes for related data
    if (include != null)
    {
      query = include(query);
    }

    // Apply filtering
    if (predicate != null)
    {
      query = query.Where(predicate);
    }

    // Apply cursor filtering if provided
    if (cursor.HasValue)
    {
      var cursorTimestamp = cursor.Value.timestamp;
      var cursorId = cursor.Value.id;

      if (forward)
      {
        // Forward: WHERE (timestamp < cursor) OR (timestamp = cursor AND id < cursor.id)
        // For DESC order, this gets older items
        query = query.Where(entity =>
          EF.Property<DateTime>(entity, "CreatedAtUtc") < cursorTimestamp ||
          (EF.Property<DateTime>(entity, "CreatedAtUtc") == cursorTimestamp &&
           string.Compare(EF.Property<string>(entity, GetIdPropertyName()), cursorId) < 0)
        );
      }
      else
      {
        // Backward: WHERE (timestamp > cursor) OR (timestamp = cursor AND id > cursor.id)
        // For DESC order, this gets newer items
        query = query.Where(entity =>
          EF.Property<DateTime>(entity, "CreatedAtUtc") > cursorTimestamp ||
          (EF.Property<DateTime>(entity, "CreatedAtUtc") == cursorTimestamp &&
           string.Compare(EF.Property<string>(entity, GetIdPropertyName()), cursorId) > 0)
        );
      }
    }

    // Apply ordering
    if (forward)
    {
      // Forward: use normal ordering (DESC)
      query = orderBy(query);
    }
    else
    {
      // Backward: reverse ordering (ASC) to get items before cursor
      query = query.OrderBy(e => EF.Property<DateTime>(e, "CreatedAtUtc"))
                   .ThenBy(e => EF.Property<string>(e, GetIdPropertyName()));
    }

    // Fetch pageSize + 1 to determine if there are more items
    var items = await query
      .Take(pageSize + 1)
      .ToListAsync(cancellationToken);

    // Check if there are more items
    var hasMore = items.Count > pageSize;

    // Remove the extra item if present
    if (hasMore)
    {
      items.RemoveAt(items.Count - 1);
    }

    // Reverse results if going backward (to maintain DESC order)
    if (!forward)
    {
      items = items.Reverse().ToList();
    }

    // HasPrevious is true if we have a cursor (not first page)
    var hasPrevious = cursor.HasValue;

    return (items, hasMore, hasPrevious);
  }

  private string GetIdPropertyName()
  {
    var keyProperty = _dbSet.EntityType.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
    if (keyProperty == null)
    {
      throw new InvalidOperationException("No primary key defined for the entity.");
    }
    return keyProperty;
  }

  public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(entity);
    await _dbSet.AddAsync(entity, cancellationToken);
  }

  public async Task AddRangeAsync(
    IEnumerable<TEntity> entities,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(entities);
    await _dbSet.AddRangeAsync(entities, cancellationToken);
  }

  public void Update(TEntity entity, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(entity);

    var entry = _context.Entry(entity);
    _dbSet.Attach(entity);

    entry.State = EntityState.Modified;
    _dbSet.Update(entity);
  }

  // Update multiple entities
  public void UpdateRange(
    IEnumerable<TEntity> entities,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(entities);
    _dbSet.UpdateRange(entities);
  }

  public void Delete(TEntity entity, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(entity);

    if (entity is ISoftDelete softDeletable)
    {
      softDeletable.DeletedAtUtc = DateTime.UtcNow;
      Update(entity, cancellationToken);
    }
    else
    {
      _dbSet.Remove(entity);
    }
  }

  public void DeleteRange(
    IEnumerable<TEntity> entities,
    CancellationToken cancellationToken = default
  )
  {
    foreach (var entity in entities)
    {
      Delete(entity, cancellationToken);
    }
  }

  public async Task<int> DeleteWhereAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default
  )
  {
    var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    foreach (var entity in entities)
    {
      Delete(entity, cancellationToken);
    }

    return entities.Count;
  }

  public void Restore(TEntity entity, CancellationToken cancellationToken = default)
  {
    if (entity is ISoftDelete softDeletable)
    {
      softDeletable.DeletedAtUtc = null;
      Update(entity, cancellationToken);
    }
  }

  public void RestoreRange(
    IEnumerable<TEntity> entities,
    CancellationToken cancellationToken = default
  )
  {
    foreach (var entity in entities)
    {
      Restore(entity, cancellationToken);
    }
  }

  public IQueryable<TEntity> Table => _dbSet.AsTracking();
  public IQueryable<TEntity> TableNoTracking => _dbSet.AsNoTracking();
}
