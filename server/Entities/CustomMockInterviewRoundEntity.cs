using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class CustomMockInterviewRoundEntity : IBaseEntity, ISoftDelete
{
  [Required]
  public string CustomMockInterviewRoundId { get; set; } = string.Empty;

  public string Content { get; set; } = string.Empty;

  public string Link { get; set; } = string.Empty;

  [Required]
  public bool IsReviewed { get; set; }

  [Required]
  public int Score { get; set; }

  [Required]
  public DateTime CreatedAtUtc { get; set; }

  [Required]
  public DateTime UpdatedAtUtc { get; set; }

  public DateTime? DeletedAtUtc { get; set; }
}

public class CustomMockInterviewRoundConfiguration
  : IEntityTypeConfiguration<CustomMockInterviewRoundEntity>
{
  public void Configure(EntityTypeBuilder<CustomMockInterviewRoundEntity> builder)
  {
    builder.ToTable("CustomMockInterviewRound");
    builder.HasKey(x => x.CustomMockInterviewRoundId);

    // Fields
    builder
      .Property(x => x.CustomMockInterviewRoundId)
      .HasColumnName("CustomMockInterviewRoundId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder
      .Property(x => x.Content)
      .HasColumnName("Content")
      .HasColumnType("varchar(10000)");
    builder.Property(x => x.Link).HasColumnName("Link").HasColumnType("varchar(255)");
    builder.Property(x => x.Score).HasColumnName("Score").HasColumnType("int").IsRequired();
    builder
      .Property(x => x.IsReviewed)
      .HasColumnName("IsReviewed")
      .IsRequired();
    builder
      .Property(x => x.CreatedAtUtc)
      .HasColumnName("CreatedAtUtc")
      .HasColumnType("timestamptz")
      .IsRequired();
    builder
      .Property(x => x.UpdatedAtUtc)
      .HasColumnName("UpdatedAtUtc")
      .HasColumnType("timestamptz")
      .IsRequired();
    builder
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");
  }
}
