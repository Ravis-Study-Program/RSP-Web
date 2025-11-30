using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Database.Helpers;

public static class AuditHelper
{
    public static async Task<List<AuditEventEntity>> GenerateAuditEntries(string userId, List<EntityEntry> entries)
    {
        return [.. entries
         .Where(entry =>
            entry.Entity is not AuditEventEntity &&
            entry.State != EntityState.Detached &&
            entry.State != EntityState.Unchanged
         )
         .Select(entry => GenerateAuditEvent(entry, userId).ToAuditEventEntity())];
    }

    private static AuditEntry GenerateAuditEvent(EntityEntry entry, string userId)
    {
        var auditEntry = new AuditEntry(entry)
        {
            // Table name could only be null if we haven't configured this entity's table in EF
            TableName = entry.Metadata.GetTableName() ?? "TABLE_NOT_MAPPED",
            ModifiedByUserId = userId
        };

        foreach (var property in entry.Properties)
        {
            string propertyName = property.Metadata.Name;

            if (property.Metadata.IsPrimaryKey())
            {
                auditEntry.AffectedEntityKey = property.CurrentValue?.ToString() ?? "NULL";
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                    break;

                case EntityState.Deleted:
                    auditEntry.OldValues[propertyName] = property.OriginalValue;
                    break;

                case EntityState.Modified:
                    // If entity state modified we still need to check if this specific property is modified or not
                    if (property.IsModified)
                    {
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                    }
                    break;
            }
        }

        return auditEntry;
    }
}

internal sealed class AuditEntry(EntityEntry entry)
{
    public EntityEntry Entry { get; } = entry;
    public string ModifiedByUserId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string AffectedEntityKey { get; set; } = string.Empty;
    public Dictionary<string, object?> OldValues { get; } = [];
    public Dictionary<string, object?> NewValues { get; } = [];

    public AuditEventEntity ToAuditEventEntity()
    {
        var audit = new AuditEventEntity
        {
            AuditId = Constants.GeneratePrimaryKeyId(),
            ModifiedByUserId = ModifiedByUserId,
            TableName = TableName,
            AffectedEntityKey = AffectedEntityKey,
            AuditedAtUtc = DateTime.UtcNow,
        };

        var changes = new List<ChangeState>();
        var allProperties = OldValues.Keys.Union(NewValues.Keys);

        foreach (var propertyName in allProperties)
        {
            OldValues.TryGetValue(propertyName, out var oldValue);
            NewValues.TryGetValue(propertyName, out var newValue);
            changes.Add(new ChangeState { Column = propertyName, OldValue = oldValue, NewValue = newValue });
        }

        audit.ChangeState = JsonSerializer.Serialize(changes);
        return audit;
    }
}

internal sealed record ChangeState
{
    public string Column { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}

