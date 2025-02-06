namespace RSPWebAPI.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
  IRepository<TEntity> GetRepository<TEntity>()
    where TEntity : class;

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
  Task BeginTransactionAsync(CancellationToken cancellationToken = default);
  Task CommitTransactionAsync(CancellationToken cancellationToken = default);
  Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
