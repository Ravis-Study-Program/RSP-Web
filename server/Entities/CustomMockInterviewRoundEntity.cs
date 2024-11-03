using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class CustomMockInterviewRoundEntity
{
  public string CustomMockInterviewRoundId { get; set; } = string.Empty;
  public string MockInterviewId { get; set; } = string.Empty;
  public string MockInterviewRoundId { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public string Link { get; set; } = string.Empty;
  public int Score { get; set; }

  // Navigation
  public virtual MockInterviewEntity? MockInterview { get; set; }
  public virtual MockInterviewRoundEntity? MockInterviewRound { get; set; }
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
    builder.Property(x => x.MockInterviewId).HasColumnName("MockInterviewId").HasColumnType("varchar(16)")
           .IsRequired();
    builder.Property(x => x.MockInterviewRoundId).HasColumnName("MockInterviewRoundId").HasColumnType("varchar(16)").IsRequired();
    builder.Property(x => x.Content).HasColumnName("Content").HasColumnType("varchar(10000)").IsRequired();
    builder.Property(x => x.Link).HasColumnName("Link").HasColumnType("varchar(255)");
    builder.Property(x => x.Score).HasColumnName("Score").HasColumnType("int").IsRequired();

    // Foreign Keys
    builder.HasOne(x => x.MockInterview).WithMany().HasForeignKey(x => x.MockInterviewId)
           .OnDelete(DeleteBehavior.Cascade).IsRequired();
    builder.HasOne(x => x.MockInterviewRound).WithOne()
           .HasForeignKey<CustomMockInterviewRoundEntity>(x => x.MockInterviewRoundId)
           .OnDelete(DeleteBehavior.Cascade).IsRequired();
  }
}