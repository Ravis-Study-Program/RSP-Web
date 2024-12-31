using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class LeetcodeProblemRecommendationEntity : ISoftDelete
{
  [Required]
  public string LeetcodeProblemRecommendationId { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public string LeetcodeProblemId { get; set; } = string.Empty;

  public string? ProblemAttemptId { get; set; }

  // Navigation
  public UserEntity User { get; set; } = null!;
  public LeetcodeProblemEntity LeetcodeProblem { get; set; } = null!;
  public ProblemAttemptEntity ProblemAttempt { get; set; } = null!;
  public DateTime? DeletedAtUtc { get; set; }
}

public class LeetcodeProblemRecommendationEntityConfiguration
  : IEntityTypeConfiguration<LeetcodeProblemRecommendationEntity>
{
  public void Configure(EntityTypeBuilder<LeetcodeProblemRecommendationEntity> builder)
  {
    builder.ToTable("LeetcodeProblemRecommendation");
    builder.HasKey(x => x.LeetcodeProblemRecommendationId);

    // Fields
    builder
      .Property(x => x.LeetcodeProblemRecommendationId)
      .HasColumnName("LeetcodeProblemRecommendationId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder
      .Property(x => x.UserId)
      .HasColumnName("UserId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.LeetcodeProblemId)
      .HasColumnName("LeetcodeProblemId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.ProblemAttemptId)
      .HasColumnName("ProblemAttemptId")
      .HasColumnType("varchar(16)");
    builder
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");

    // Foreign Keys
    builder
      .HasOne(x => x.User)
      .WithMany()
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
    builder
      .HasOne(x => x.LeetcodeProblem)
      .WithMany()
      .HasForeignKey(x => x.LeetcodeProblemId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
    builder
      .HasOne(x => x.ProblemAttempt)
      .WithMany()
      .HasForeignKey(x => x.ProblemAttemptId)
      .OnDelete(DeleteBehavior.SetNull)
      .IsRequired(false);
  }
}
