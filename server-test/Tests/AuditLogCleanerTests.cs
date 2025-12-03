using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Jobs;
using RSPWebAPI.Tests.Shared;
using Xunit;

namespace RSPWebAPI.Tests.Tests;

public class AuditLogCleanerTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;

    public AuditLogCleanerTests(IntegrationTestWebAppFactory factory)
      : base(factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabase();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Removes_Audit_Records_Older_Than_Three_Months()
    {
        var oldAuditEvent = new AuditEventEntity
        {
            AuditId = Constants.GeneratePrimaryKeyId(),
            AuditedAtUtc = DateTime.UtcNow.AddMonths(-4),
            ChangeState = "[]"
        };

        var newAuditEvent = new AuditEventEntity
        {
            AuditId = Constants.GeneratePrimaryKeyId(),
            AuditedAtUtc = DateTime.UtcNow.AddMonths(-1),
            ChangeState = "[]"
        };

        await DbContext.AuditEvents.AddRangeAsync(oldAuditEvent, newAuditEvent);
        await DbContext.SaveChangesAsync();

        var cleaner = new AuditLogCleaner(_factory.Services, TimeSpan.FromMilliseconds(1));
        await cleaner.StartAsync(CancellationToken.None);

        // Allow the background job to run to completion
        await Task.Delay(100);

        await cleaner.StopAsync(CancellationToken.None);

        Assert.True(cleaner.ExecuteTask?.IsCompleted);

        var auditEvents = await DbContext.AuditEvents.ToListAsync();

        Assert.Single(auditEvents);
        Assert.Equal(newAuditEvent.AuditId, auditEvents.First().AuditId);
    }
}