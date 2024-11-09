using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class BehaviouralMockInterviewRoundEntity
{
  [Required] public string BehaviouralMockInterviewRoundId { get; set; } = string.Empty;
  [Required] public int BehavioralScore { get; set; }
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
    builder.Property(x => x.BehavioralScore).HasColumnName("BehavioralScore").HasColumnType("int").IsRequired();
  }
}
