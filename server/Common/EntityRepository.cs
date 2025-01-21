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
