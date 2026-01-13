using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;

namespace RSPWebAPI.Jobs;

public class AuditLogCleaner(IServiceProvider serviceProvider, TimeSpan? interval = null) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly TimeSpan _interval = interval ?? TimeSpan.FromDays(1);
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[Audit Log Cleaner] Scheduled job started");

        // Will execute roughly once every 3 months which corresponds to a standard RSP season
        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var cutoffDate = DateTime.UtcNow.AddMonths(-3);

                await using var scope = _serviceProvider.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var deletedCount = await dbContext.AuditEvents
                    .Where(e => e.AuditedAtUtc < cutoffDate)
                    .ExecuteDeleteAsync(cancellationToken);

                Console.WriteLine($"[Audit Log Cleaner] Deleted {deletedCount} audit events");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Audit Log Cleaner] Cleanup task was cancelled");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Audit Log Cleaner] An error occured: {ex.Message}");
            }
        }
    }
}
