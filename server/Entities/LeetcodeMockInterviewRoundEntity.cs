using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class LeetcodeMockInterviewRoundEntity
{
  public string LeetcodeMockInterviewRoundId { get; set; } = string.Empty;
  public string MockInterviewId { get; set; } = string.Empty;
  public string MockInterviewRoundId { get; set; } = string.Empty;
  public string LeetcodeProblemId { get; set; } = string.Empty;

  public int ConfirmQuestionScore { get; set; }
  public int AlgorithmDesignScore { get; set; }
  public int ComplexityAnalysisScore { get; set; }
  public int CodingScore { get; set; }
  public int TestingScore { get; set; }

  // Navigation
  public virtual MockInterviewEntity? MockInterview { get; set; }
  public virtual MockInterviewRoundEntity? MockInterviewRound { get; set; }
  public virtual LeetcodeProblemEntity? LeetcodeProblem { get; set; }
}

public class LeetcodeMockInterviewRoundEntityConfiguration : IEntityTypeConfiguration<LeetcodeMockInterviewRoundEntity>
{
  public void Configure(EntityTypeBuilder<LeetcodeMockInterviewRoundEntity> builder)
  {
    builder.ToTable("LeetcodeMockInterviewRound");
    builder.HasKey(x => x.LeetcodeMockInterviewRoundId);

    // Fields
    builder.Property(x => x.LeetcodeMockInterviewRoundId).HasColumnName("LeetcodeMockInterviewRoundId")
           .HasColumnType("varchar(32)").ValueGeneratedNever().IsRequired();
    builder.Property(x => x.MockInterviewId).HasColumnName("MockInterviewId").HasColumnType("varchar(16)")
           .IsRequired();
    builder.Property(x => x.MockInterviewRoundId).HasColumnName("MockInterviewRoundId").HasColumnType("varchar(16)").IsRequired();
    builder.Property(x => x.LeetcodeProblemId).HasColumnName("LeetcodeProblemId").HasColumnType("varchar(16)").IsRequired();
    builder.Property(x => x.ConfirmQuestionScore).HasColumnName("ConfirmQuestionScore").HasColumnType("int")
           .IsRequired();
    builder.Property(x => x.AlgorithmDesignScore).HasColumnName("AlgorithmDesignScore").HasColumnType("int")
           .IsRequired();
    builder.Property(x => x.ComplexityAnalysisScore).HasColumnName("ComplexityAnalysisScore").HasColumnType("int")
           .IsRequired();
    builder.Property(x => x.CodingScore).HasColumnName("CodingScore").HasColumnType("int").IsRequired();
    builder.Property(x => x.TestingScore).HasColumnName("TestingScore").HasColumnType("int").IsRequired();

    // Foreign Keys
    builder.HasOne(x => x.MockInterview).WithMany().HasForeignKey(x => x.MockInterviewId)
           .OnDelete(DeleteBehavior.Cascade).IsRequired();
    builder.HasOne(x => x.MockInterviewRound).WithOne()
           .HasForeignKey<LeetcodeMockInterviewRoundEntity>(x => x.MockInterviewRoundId)
           .OnDelete(DeleteBehavior.Cascade).IsRequired();
    builder.HasOne(x => x.LeetcodeProblem).WithMany().HasForeignKey(x => x.LeetcodeProblemId)
           .OnDelete(DeleteBehavior.Restrict).IsRequired();
  }
}
