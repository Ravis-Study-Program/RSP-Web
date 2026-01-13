using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class AuditEventEntity
{
  public string AuditId { get; set; } = string.Empty;
  public string ModifiedByUserId { get; set; } = string.Empty;
  public DateTime AuditedAtUtc { get; set; }
  public string TableName { get; set; } = string.Empty;
  public string AffectedEntityKey { get; set; } = string.Empty;
  public string ChangeState { get; set; } = string.Empty;
}

public class AuditEventEntityConfiguration : IEntityTypeConfiguration<AuditEventEntity>
{
  public void Configure(EntityTypeBuilder<AuditEventEntity> builder)
  {
    builder.ToTable("AuditEvent");
    builder.HasKey(x => x.AuditId);

    // Indexes
    builder.HasIndex(x => new { x.TableName, x.AffectedEntityKey });

    // Fields
    builder
    .Property(x => x.AuditId)
    .HasColumnName("AuditId")
    .HasColumnType("varchar(16)")
    .ValueGeneratedNever()
    .IsRequired();
    builder
     .Property(x => x.ModifiedByUserId)
     .HasColumnName("ModifiedByUserId")
     .HasColumnType("varchar(16)")
     .IsRequired();
    builder
     .Property(x => x.AuditedAtUtc)
     .HasColumnName("AuditedAtUtc")
     .HasColumnType("timestamptz")
     .IsRequired();
    builder.Property(x => x.TableName)
     .HasColumnName("TableName")
     .HasColumnType("varchar(100)")
     .IsRequired();
    builder
     .Property(x => x.AffectedEntityKey)
     .HasColumnName("AffectedEntityKey")
     .HasColumnType("varchar(16)")
     .IsRequired();
    builder
     .Property(x => x.ChangeState)
     .HasColumnName("ChangeState")
     .HasColumnType("jsonb")
     .IsRequired();
  }
}
