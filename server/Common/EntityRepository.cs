using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities.Interfaces;
using server.Shared;

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

  public async Task<PaginatedResponse<TEntity>> GetPagedWithCursorAsync(
    CursorPaginationOptions<TEntity> options,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(options);

    IQueryable<TEntity> query = TableNoTracking;

    // Apply includes for related data
    if (options.Include != null)
    {
      query = options.Include(query);
    }

    // Apply filtering
    if (options.Predicate != null)
    {
      query = query.Where(options.Predicate);
    }

    // Apply cursor filtering if provided
    if (options.Cursor != null)
    {
      var cursorTimestamp = options.Cursor.CreatedAtUtc;
      var cursorId = options.Cursor.Id;

      if (options.Forward)
      {
        // Forward pagination with DESC ordering: get items older than cursor
        // WHERE (timestamp < cursor) OR (timestamp = cursor AND id < cursor.id)
        query = query.Where(entity =>
          EF.Property<DateTime>(entity, CursorPaginationConstants.TimestampPropertyName) < cursorTimestamp ||
          (EF.Property<DateTime>(entity, CursorPaginationConstants.TimestampPropertyName) == cursorTimestamp &&
           string.Compare(EF.Property<string>(entity, GetIdPropertyName()), cursorId) < 0)
        );
      }
      else
      {
        // Backward pagination with DESC ordering: get items newer than cursor
        // WHERE (timestamp > cursor) OR (timestamp = cursor AND id > cursor.id)
        query = query.Where(entity =>
          EF.Property<DateTime>(entity, CursorPaginationConstants.TimestampPropertyName) > cursorTimestamp ||
          (EF.Property<DateTime>(entity, CursorPaginationConstants.TimestampPropertyName) == cursorTimestamp &&
           string.Compare(EF.Property<string>(entity, GetIdPropertyName()), cursorId) > 0)
        );
      }
    }

    // Apply ordering: DESC on timestamp, then DESC on primary key
    if (options.Forward)
    {
      // Forward: normal DESC ordering
      query = query
        .OrderByDescending(e => EF.Property<DateTime>(e, CursorPaginationConstants.TimestampPropertyName))
        .ThenByDescending(e => EF.Property<string>(e, GetIdPropertyName()));
    }
    else
    {
      // Backward: ASC ordering (will be reversed later to maintain DESC result order)
      query = query
        .OrderBy(e => EF.Property<DateTime>(e, CursorPaginationConstants.TimestampPropertyName))
        .ThenBy(e => EF.Property<string>(e, GetIdPropertyName()));
    }

    // Fetch pageSize + 1 to determine if there are more items
    var items = await query
      .Take(options.PageSize + 1)
      .ToListAsync(cancellationToken);

    // Check if there are more items
    var hasMore = items.Count > options.PageSize;

    // Remove the extra item if present
    if (hasMore)
    {
      items.RemoveAt(items.Count - 1);
    }

    // Reverse results if going backward (to maintain DESC order)
    if (!options.Forward)
    {
      items.Reverse();
    }

    // HasPrevious is true if we have a cursor (not first page)
    var hasPrevious = options.Cursor != null;

    return new PaginatedResponse<TEntity>
    {
      Items = items,
      HasMore = hasMore,
      HasPrevious = hasPrevious
    };
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
