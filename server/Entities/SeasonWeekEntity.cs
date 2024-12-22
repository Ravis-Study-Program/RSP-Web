using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class SeasonWeekEntity : ISoftDelete
{
  [Required]
  public string SeasonWeekId { get; set; } = string.Empty;

  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public int WeekNumber { get; set; }

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public DateTime EndDate { get; set; }

  // Navigation
  public SeasonEntity Season { get; set; } = null!;
  public DateTime? DeletedAtUtc { get; set; }
}

public class SeasonWeekEntityConfiguration : IEntityTypeConfiguration<SeasonWeekEntity>
{
  public void Configure(EntityTypeBuilder<SeasonWeekEntity> builder)
  {
    builder.ToTable("SeasonWeek");
    builder.HasKey(x => x.SeasonWeekId);

    // Fields
    builder
      .Property(x => x.SeasonWeekId)
      .HasColumnName("SeasonWeekId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();

    builder
      .Property(x => x.SeasonId)
      .HasColumnName("SeasonId")
      .HasColumnType("varchar(16)")
      .IsRequired();

    builder
      .Property(x => x.WeekNumber)
      .HasColumnName("WeekNumber")
      .HasColumnType("integer")
      .IsRequired();

    builder
      .Property(x => x.StartDate)
      .HasColumnName("StartDate")
      .HasColumnType("timestamptz")
      .IsRequired();

    builder
      .Property(x => x.EndDate)
      .HasColumnName("EndDate")
      .HasColumnType("timestamptz")
      .IsRequired();

    builder
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");

    // Foreign Keys
    builder
      .HasOne(x => x.Season)
      .WithMany()
      .HasForeignKey(x => x.SeasonId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
  }
}
