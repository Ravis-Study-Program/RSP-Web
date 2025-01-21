using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RSPWebAPI.Common.Interfaces;

namespace RSPWebAPI.Common;

public class UnitOfWork : IUnitOfWork
{
  private readonly DbContext _context;
  private readonly Dictionary<Type, object> _repositories;
  private IDbContextTransaction? _currentTransaction;

  public UnitOfWork(DbContext context)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
    _repositories = new Dictionary<Type, object>();
    _currentTransaction = null;
  }

  public IRepository<TEntity> GetRepository<TEntity>()
    where TEntity : class
  {
    if (_repositories.TryGetValue(typeof(TEntity), out var repository))
    {
      return (IRepository<TEntity>)repository;
    }

    var newRepository = new EntityRepository<TEntity>(_context);
    _repositories[typeof(TEntity)] = newRepository;
    return newRepository;
  }

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    return await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task BeginTransactionAsync()
  {
    if (_currentTransaction != null)
    {
      throw new InvalidOperationException("A transaction is already in progress.");
    }

    _currentTransaction = await _context.Database.BeginTransactionAsync();
  }

  public async Task CommitTransactionAsync()
  {
    if (_currentTransaction == null)
    {
      throw new InvalidOperationException("No transaction is in progress.");
    }

    try
    {
      await _context.SaveChangesAsync();
      await _currentTransaction.CommitAsync();
    }
    finally
    {
      await DisposeTransactionAsync();
    }
  }

  public async Task RollbackTransactionAsync()
  {
    if (_currentTransaction == null)
    {
      throw new InvalidOperationException("No transaction is in progress.");
    }

    try
    {
      await _currentTransaction.RollbackAsync();
    }
    finally
    {
      await DisposeTransactionAsync();
    }
  }

  public void Dispose()
  {
    DisposeTransactionAsync().GetAwaiter().GetResult();
    _context.Dispose();
  }

  private async Task DisposeTransactionAsync()
  {
    if (_currentTransaction != null)
    {
      await _currentTransaction.DisposeAsync();
      _currentTransaction = null;
    }
  }
}
