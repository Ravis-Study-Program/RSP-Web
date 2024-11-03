using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class BehaviouralMockInterviewRoundEntity
{
  public string BehaviouralMockInterviewRoundId { get; set; } = string.Empty;
  public string MockInterviewId { get; set; } = string.Empty;
  public string MockInterviewRoundId { get; set; } = string.Empty;
  public int BehavioralScore { get; set; }

  // Navigation
  public virtual MockInterviewEntity? MockInterview { get; set; }
  public virtual MockInterviewRoundEntity? MockInterviewRound { get; set; }
}

public class
  BehaviouralMockInterviewRoundEntityConfiguration : IEntityTypeConfiguration<BehaviouralMockInterviewRoundEntity>
{
  public void Configure(EntityTypeBuilder<BehaviouralMockInterviewRoundEntity> builder)
  {
    builder.ToTable("BehaviouralMockInterviewRound");
    builder.HasKey(x => x.BehaviouralMockInterviewRoundId);

    // Fields
    builder.Property(x => x.BehaviouralMockInterviewRoundId).HasColumnName("BehaviouralMockInterviewRoundId")
           .HasColumnType("varchar(32)").ValueGeneratedNever().IsRequired();
    builder.Property(x => x.MockInterviewId).HasColumnName("MockInterviewId").HasColumnType("varchar(16)")
           .IsRequired();
    builder.Property(x => x.MockInterviewRoundId).HasColumnName("MockInterviewRoundId").HasColumnType("varchar(16)")
           .IsRequired();
    builder.Property(x => x.BehavioralScore).HasColumnName("BehavioralScore").HasColumnType("int").IsRequired();

    // Foreign Keys 
    builder.HasOne(x => x.MockInterview).WithMany().HasForeignKey(x => x.MockInterviewId)
           .OnDelete(DeleteBehavior.Cascade).IsRequired();
    builder.HasOne(x => x.MockInterviewRound).WithOne()
           .HasForeignKey<BehaviouralMockInterviewRoundEntity>(x => x.MockInterviewRoundId)
           .OnDelete(DeleteBehavior.Cascade).IsRequired();
  }
}
