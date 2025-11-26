using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Database.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
    DbContextEventData eventData,
    InterceptionResult<int> result,
    CancellationToken cancellationToken = default
  )
  {
    if (eventData.Context is null)
    {
      return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    var now = DateTime.UtcNow;

    foreach (var entry in eventData.Context.ChangeTracker.Entries<IBaseEntity>())
    {
      if (entry.State == EntityState.Added)
      {
        entry.Entity.CreatedAtUtc = now;
        entry.Entity.UpdatedAtUtc = now;
      }
      else if (entry.State == EntityState.Modified)
      {
        entry.Entity.UpdatedAtUtc = now;
      }
    }

    return base.SavingChangesAsync(eventData, result, cancellationToken);
  }
}
