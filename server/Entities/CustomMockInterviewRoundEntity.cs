using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class CustomMockInterviewRoundEntity
{
  [Required] public string CustomMockInterviewRoundId { get; set; } = string.Empty;
  [Required] public string Content { get; set; } = string.Empty;
  [Required] public string Link { get; set; } = string.Empty;
  [Required] public int Score { get; set; }
}

public class CustomMockInterviewRoundConfiguration : IEntityTypeConfiguration<CustomMockInterviewRoundEntity>
{
  public void Configure(EntityTypeBuilder<CustomMockInterviewRoundEntity> builder)
  {
    builder.ToTable("CustomMockInterviewRound");
    builder.HasKey(x => x.CustomMockInterviewRoundId);

    // Fields
    builder.Property(x => x.CustomMockInterviewRoundId).HasColumnName("CustomMockInterviewRoundId")
           .HasColumnType("varchar(32)").ValueGeneratedNever().IsRequired();
    builder.Property(x => x.Content).HasColumnName("Content").HasColumnType("varchar(10000)").IsRequired();
    builder.Property(x => x.Link).HasColumnName("Link").HasColumnType("varchar(255)");
    builder.Property(x => x.Score).HasColumnName("Score").HasColumnType("int").IsRequired();
  }
}
