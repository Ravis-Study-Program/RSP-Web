using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;

namespace RSPWebAPI.Features.BackgroundServices;

public class UpdateGraduateStatusService : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<UpdateGraduateStatusService> _logger;

  public UpdateGraduateStatusService(
    IServiceProvider serviceProvider,
    ILogger<UpdateGraduateStatusService> logger
  )
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        await UpdateGraduateStatusAsync(stoppingToken);
        
        // Wait for 24 hours before the next update
        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
      }
      catch (OperationCanceledException)
      {
        // Service is shutting down
        break;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while updating graduate status");
        
        // Wait 1 hour before retrying on error
        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
      }
    }
  }

  private async Task UpdateGraduateStatusAsync(CancellationToken cancellationToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    _logger.LogInformation("Starting graduate status update job");

    // Update all users with correct graduate status in a single query
    var updatedCount = await dbContext.Database.ExecuteSqlRawAsync(@"
      UPDATE ""User"" 
      SET ""IsGraduate"" = CASE 
        WHEN ""UserId"" IN (
          SELECT DISTINCT e.""UserId"" 
          FROM ""Enrollment"" e 
          INNER JOIN ""Season"" s ON e.""SeasonId"" = s.""SeasonId"" 
          WHERE s.""EndDateInclusiveUtc"" < NOW() 
          AND e.""DeletedAtUtc"" IS NULL 
          AND s.""DeletedAtUtc"" IS NULL
        ) THEN true 
        ELSE false 
      END
      WHERE ""DeletedAtUtc"" IS NULL", cancellationToken);

    _logger.LogInformation("Graduate status update completed. Updated {UpdatedCount} users to graduate status", updatedCount);
  }

  public override Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Graduate status update service is starting");
    return base.StartAsync(cancellationToken);
  }

  public override Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Graduate status update service is stopping");
    return base.StopAsync(cancellationToken);
  }
}