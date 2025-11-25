using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class LeetcodeProblemCategoryEntity : IBaseEntity, ISoftDelete
{
  [Required]
  public string LeetcodeProblemCategoryId { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public DateTime CreatedAtUtc { get; set; }

  [Required]
  public DateTime UpdatedAtUtc { get; set; }

  public DateTime? DeletedAtUtc { get; set; }
}

public class LeetcodeProblemCategoryEntityConfiguration
  : IEntityTypeConfiguration<LeetcodeProblemCategoryEntity>
{
  public void Configure(EntityTypeBuilder<LeetcodeProblemCategoryEntity> builder)
  {
    builder.ToTable("LeetcodeProblemCategory");
    builder.HasKey(x => x.LeetcodeProblemCategoryId);

    // Fields
    builder
      .Property(x => x.LeetcodeProblemCategoryId)
      .HasColumnName("LeetcodeProblemCategoryId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder.Property(x => x.Name).HasColumnName("Name").HasColumnType("varchar(100)").IsRequired();
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
