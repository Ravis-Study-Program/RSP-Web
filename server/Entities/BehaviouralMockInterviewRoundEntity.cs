using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class BehaviouralMockInterviewRoundEntity : ISoftDelete
{
  [Required] public string BehaviouralMockInterviewRoundId { get; set; } = string.Empty;
  [Required] public int BehavioralScore { get; set; }
  public DateTime? DeletedAtUtc { get; set; }
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
           .HasColumnType("varchar(16)").ValueGeneratedNever().IsRequired();
    builder.Property(x => x.BehavioralScore).HasColumnName("BehavioralScore").HasColumnType("int").IsRequired();
    builder.Property(x => x.DeletedAtUtc).HasColumnName("DeletedAtUtc").HasColumnType("timestamptz");
  }
}
