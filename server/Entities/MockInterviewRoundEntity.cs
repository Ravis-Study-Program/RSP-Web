using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class MockInterviewRoundEntity : ISoftDelete
{
  [Required] public string MockInterviewRoundId { get; set; } = string.Empty;
  [Required] public string MockInterviewId { get; set; } = string.Empty;
  [Required] public bool IsReviewedByInterviewee { get; set; }
  [Required] public string IntervieweeComment { get; set; } = string.Empty;

  // The actual mock interview could be one of the following listed below.
  public string? BehaviouralMockInterviewRoundId { get; set; }
  public string? LeetcodeMockInterviewRoundId { get; set; }
  public string? CustomMockInterviewRoundId { get; set; }

  // Navigation
  [JsonIgnore] public MockInterviewEntity MockInterview { get; set; } = null!;
  public BehaviouralMockInterviewRoundEntity BehaviouralMockInterviewRound { get; set; } = null!;
  public LeetcodeMockInterviewRoundEntity LeetcodeMockInterviewRound { get; set; } = null!;
  public CustomMockInterviewRoundEntity CustomMockInterviewRound { get; set; } = null!;
  public DateTime? DeletedAtUtc { get; set; }
}

public class MockInterviewRoundEntityConfiguration : IEntityTypeConfiguration<MockInterviewRoundEntity>
{
  public void Configure(EntityTypeBuilder<MockInterviewRoundEntity> builder)
  {
    builder.ToTable("MockInterviewRound");
    builder.HasKey(x => x.MockInterviewRoundId);

    // Fields
    builder.Property(x => x.MockInterviewRoundId).HasColumnName("MockInterviewRoundId").HasColumnType("varchar(16)")
           .ValueGeneratedNever().IsRequired();
    builder.Property(x => x.MockInterviewId).HasColumnName("MockInterviewId").HasColumnType("varchar(16)")
           .IsRequired();
    builder.Property(x => x.IsReviewedByInterviewee).HasColumnName("IsReviewedByInterviewee").IsRequired();
    builder.Property(x => x.IntervieweeComment).HasColumnName("IntervieweeComment").HasColumnType("varchar(1000)")
           .IsRequired();
    builder.Property(x => x.BehaviouralMockInterviewRoundId).HasColumnName("BehaviouralMockInterviewRoundId")
           .HasColumnType("varchar(16)");
    builder.Property(x => x.LeetcodeMockInterviewRoundId).HasColumnName("LeetcodeMockInterviewRoundId")
           .HasColumnType("varchar(16)");
    builder.Property(x => x.CustomMockInterviewRoundId).HasColumnName("CustomMockInterviewRoundId")
           .HasColumnType("varchar(16)");
    builder.Property(x => x.DeletedAtUtc).HasColumnName("DeletedAtUtc").HasColumnType("timestamptz");

    // Foreign Keys
    builder.HasOne(x => x.MockInterview)
           .WithMany(x => x.MockInterviewRounds)
           .HasForeignKey(x => x.MockInterviewId)
           .OnDelete(DeleteBehavior.Cascade)
           .IsRequired();

    builder.HasOne(x => x.BehaviouralMockInterviewRound)
           .WithOne()
           .HasForeignKey<MockInterviewRoundEntity>(x => x.BehaviouralMockInterviewRoundId)
           .OnDelete(DeleteBehavior.SetNull)
           .IsRequired(false);

    builder.HasOne(x => x.LeetcodeMockInterviewRound)
           .WithOne()
           .HasForeignKey<MockInterviewRoundEntity>(x => x.LeetcodeMockInterviewRoundId)
           .OnDelete(DeleteBehavior.SetNull)
           .IsRequired(false);

    builder.HasOne(x => x.CustomMockInterviewRound)
           .WithOne()
           .HasForeignKey<MockInterviewRoundEntity>(x => x.CustomMockInterviewRoundId)
           .OnDelete(DeleteBehavior.SetNull)
           .IsRequired(false);
  }
}
