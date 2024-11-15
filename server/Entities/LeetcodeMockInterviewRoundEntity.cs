using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class LeetcodeMockInterviewRoundEntity : ISoftDelete
{
  [Required] public string LeetcodeMockInterviewRoundId { get; set; } = string.Empty;
  [Required] public string LeetcodeProblemId { get; set; } = string.Empty;

  [Required] public int ConfirmQuestionScore { get; set; }
  [Required] public int AlgorithmDesignScore { get; set; }
  [Required] public int ComplexityAnalysisScore { get; set; }
  [Required] public int CodingScore { get; set; }
  [Required] public int TestingScore { get; set; }

  // Navigation
  public LeetcodeProblemEntity LeetcodeProblem { get; set; } = null!;
  public DateTime? DeletedAtUtc { get; set; }
}

public class LeetcodeMockInterviewRoundEntityConfiguration : IEntityTypeConfiguration<LeetcodeMockInterviewRoundEntity>
{
  public void Configure(EntityTypeBuilder<LeetcodeMockInterviewRoundEntity> builder)
  {
    builder.ToTable("LeetcodeMockInterviewRound");
    builder.HasKey(x => x.LeetcodeMockInterviewRoundId);

    // Fields
    builder.Property(x => x.LeetcodeMockInterviewRoundId).HasColumnName("LeetcodeMockInterviewRoundId")
           .HasColumnType("varchar(16)").ValueGeneratedNever().IsRequired();
    builder.Property(x => x.LeetcodeProblemId).HasColumnName("LeetcodeProblemId").HasColumnType("varchar(16)")
           .IsRequired();
    builder.Property(x => x.ConfirmQuestionScore).HasColumnName("ConfirmQuestionScore").HasColumnType("int")
           .IsRequired();
    builder.Property(x => x.AlgorithmDesignScore).HasColumnName("AlgorithmDesignScore").HasColumnType("int")
           .IsRequired();
    builder.Property(x => x.ComplexityAnalysisScore).HasColumnName("ComplexityAnalysisScore").HasColumnType("int")
           .IsRequired();
    builder.Property(x => x.CodingScore).HasColumnName("CodingScore").HasColumnType("int").IsRequired();
    builder.Property(x => x.TestingScore).HasColumnName("TestingScore").HasColumnType("int").IsRequired();
    builder.Property(x => x.DeletedAtUtc).HasColumnName("DeletedAtUtc").HasColumnType("timestamptz");

    // Foreign Keys
    builder.HasOne(x => x.LeetcodeProblem).WithMany().HasForeignKey(x => x.LeetcodeProblemId)
           .OnDelete(DeleteBehavior.Restrict).IsRequired();
  }
}
