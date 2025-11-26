using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class LeetcodeProblemEntity : IBaseEntity, ISoftDelete
{
  [Required]
  public string LeetcodeProblemId { get; set; } = string.Empty;

  [Required]
  public string ProblemId { get; set; } = string.Empty;

  [Required]
  public int LeetcodeNumber { get; set; }

  [Required]
  public LeetcodeProblemDifficulty LeetcodeProblemDifficulty { get; set; }

  [Required]
  public bool IsPremium { get; set; }

  // Navigation
  public ProblemEntity Problem { get; set; } = null!;
  public ICollection<LeetcodeProblemCategoryEntity> LeetcodeProblemCategories { get; set; } = null!;

  [Required]
  public DateTime CreatedAtUtc { get; set; }

  [Required]
  public DateTime UpdatedAtUtc { get; set; }

  public DateTime? DeletedAtUtc { get; set; }
}

public class LeetcodeProblemEntityConfiguration : IEntityTypeConfiguration<LeetcodeProblemEntity>
{
  public void Configure(EntityTypeBuilder<LeetcodeProblemEntity> builder)
  {
    builder.ToTable("LeetcodeProblem");
    builder.HasKey(x => x.LeetcodeProblemId);

    // Fields
    builder
      .Property(x => x.LeetcodeProblemId)
      .HasColumnName("LeetcodeProblemId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder
      .Property(x => x.ProblemId)
      .HasColumnName("ProblemId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.LeetcodeNumber)
      .HasColumnName("LeetcodeNumber")
      .HasColumnType("int")
      .IsRequired();
    builder
      .Property(x => x.LeetcodeProblemDifficulty)
      .HasColumnName("LeetcodeProblemDifficulty")
      .HasColumnType("int")
      .IsRequired();
    builder.Property(x => x.IsPremium).HasColumnName("IsPremium").IsRequired();
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

    // Foreign Keys
    builder
      .HasOne(x => x.Problem)
      .WithMany()
      .HasForeignKey(x => x.ProblemId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
    builder
      .HasMany(x => x.LeetcodeProblemCategories)
      .WithMany()
      .UsingEntity(j => j.ToTable("LeetcodeProblemCategoryMapping"));
  }
}
