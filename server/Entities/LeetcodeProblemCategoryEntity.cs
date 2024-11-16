using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class LeetcodeProblemCategoryEntity : ISoftDelete
{
  [Required]
  public string LeetcodeProblemCategoryId { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;
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
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");
  }
}
