using RSPWebAPI.Common.Interfaces;

namespace RSPWebAPI.Common;

public abstract class BaseService
{
  protected readonly IUnitOfWork _unitOfWork;
  protected readonly ILogger _logger;

  protected BaseService(IUnitOfWork unitOfWork, ILogger logger)
  {
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  /// <summary>
  /// Executes an operation and automatically saves changes to the database.
  /// Handles exceptions by logging and rethrowing as InvalidOperationException.
  /// </summary>
  protected async Task<T> ExecuteWithSaveAsync<T>(
    Func<Task<T>> operation,
    string errorMessage,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var result = await operation();
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, errorMessage);
      throw new InvalidOperationException(errorMessage);
    }
  }

  /// <summary>
  /// Executes an operation and automatically saves changes to the database.
  /// Returns void. Handles exceptions by logging and rethrowing as InvalidOperationException.
  /// </summary>
  protected async Task ExecuteWithSaveAsync(
    Func<Task> operation,
    string errorMessage,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      await operation();
      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, errorMessage);
      throw new InvalidOperationException(errorMessage);
    }
  }

  /// <summary>
  /// Executes an operation within a transaction and automatically saves changes.
  /// Handles rollback on failure and logs exceptions.
  /// </summary>
  protected async Task<T> ExecuteWithTransactionAsync<T>(
    Func<Task<T>> operation,
    string errorMessage,
    CancellationToken cancellationToken = default
  )
  {
    await _unitOfWork.BeginTransactionAsync(cancellationToken);

    try
    {
      var result = await operation();
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      await _unitOfWork.CommitTransactionAsync(cancellationToken);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, errorMessage);
      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
      throw new InvalidOperationException(errorMessage);
    }
  }

  /// <summary>
  /// Executes an operation within a transaction and automatically saves changes.
  /// Returns void. Handles rollback on failure and logs exceptions.
  /// </summary>
  protected async Task ExecuteWithTransactionAsync(
    Func<Task> operation,
    string errorMessage,
    CancellationToken cancellationToken = default
  )
  {
    await _unitOfWork.BeginTransactionAsync(cancellationToken);

    try
    {
      await operation();
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, errorMessage);
      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
      throw new InvalidOperationException(errorMessage);
    }
  }
}
