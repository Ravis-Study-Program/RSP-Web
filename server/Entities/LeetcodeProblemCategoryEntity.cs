using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class LeetcodeProblemCategoryEntity
{
  public string LeetcodeProblemCategoryId { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
}

public class LeetcodeProblemCategoryEntityConfiguration : IEntityTypeConfiguration<LeetcodeProblemCategoryEntity>
{
  public void Configure(EntityTypeBuilder<LeetcodeProblemCategoryEntity> builder)
  {
    builder.ToTable("LeetcodeProblemCategory");
    builder.HasKey(x => x.LeetcodeProblemCategoryId);

    // Fields
    builder.Property(x => x.LeetcodeProblemCategoryId).HasColumnName("LeetcodeProblemCategoryId")
           .HasColumnType("varchar(32)").ValueGeneratedNever().IsRequired();
    builder.Property(x => x.Name).HasColumnName("Name").HasColumnType("varchar(100)").IsRequired();
  }
}
