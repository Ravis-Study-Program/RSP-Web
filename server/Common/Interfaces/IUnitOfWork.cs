namespace RSPWebAPI.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
  IRepository<TEntity> GetRepository<TEntity>()
    where TEntity : class;

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
  Task BeginTransactionAsync();
  Task CommitTransactionAsync();
  Task RollbackTransactionAsync();
}
