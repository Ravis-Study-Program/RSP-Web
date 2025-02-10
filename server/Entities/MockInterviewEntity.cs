using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class MockInterviewEntity : ISoftDelete
{
  [Required]
  public string MockInterviewId { get; set; } = string.Empty;

  [Required]
  public bool IsPass { get; set; }

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public int TimeTakenInMinutes { get; set; }

  [Required]
  public string InterviewerUserId { get; set; } = string.Empty;

  [Required]
  public string IntervieweeUserId { get; set; } = string.Empty;

  // A null season and season week would mean the mock interview is not tied to any season.
  public string? SeasonId { get; set; }
  public string? SeasonWeekId { get; set; }

  // Navigation
  public UserEntity Interviewer { get; set; } = null!;
  public UserEntity Interviewee { get; set; } = null!;
  public SeasonEntity Season { get; set; } = null!;
  public ICollection<MockInterviewRoundEntity> MockInterviewRounds { get; set; } = null!;
  public SeasonWeekEntity SeasonWeek { get; set; } = null!;
  public DateTime? DeletedAtUtc { get; set; }
}

public class MockInterviewEntityConfiguration : IEntityTypeConfiguration<MockInterviewEntity>
{
  public void Configure(EntityTypeBuilder<MockInterviewEntity> builder)
  {
    builder.ToTable("MockInterview");
    builder.HasKey(x => x.MockInterviewId);

    // Fields
    builder
      .Property(x => x.MockInterviewId)
      .HasColumnName("MockInterviewId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder.Property(x => x.IsPass).HasColumnName("IsPass").IsRequired();
    builder
      .Property(x => x.StartDate)
      .HasColumnName("StartDate")
      .HasColumnType("timestamptz")
      .IsRequired();
    builder
      .Property(x => x.TimeTakenInMinutes)
      .HasColumnName("TimeTakenInMinutes")
      .HasColumnType("int")
      .IsRequired();
    builder.Property(x => x.SeasonId).HasColumnName("SeasonId").HasColumnType("varchar(16)");
    builder
      .Property(x => x.SeasonWeekId)
      .HasColumnName("SeasonWeekId")
      .HasColumnType("varchar(16)");
    builder
      .Property(x => x.InterviewerUserId)
      .HasColumnName("InterviewerUserId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.IntervieweeUserId)
      .HasColumnName("IntervieweeUserId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");

    // Foreign Keys
    builder
      .HasOne(x => x.Interviewer)
      .WithMany()
      .HasForeignKey(x => x.InterviewerUserId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
    builder
      .HasOne(x => x.Interviewee)
      .WithMany()
      .HasForeignKey(x => x.IntervieweeUserId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
    builder
      .HasOne(x => x.Season)
      .WithMany()
      .HasForeignKey(x => x.SeasonId)
      .OnDelete(DeleteBehavior.SetNull)
      .IsRequired(false);
    builder
      .HasOne(x => x.SeasonWeek)
      .WithMany()
      .HasForeignKey(x => x.SeasonWeekId)
      .OnDelete(DeleteBehavior.SetNull)
      .IsRequired(false);

    // Collection Relationship
    builder
      .HasMany(x => x.MockInterviewRounds)
      .WithOne()
      .HasForeignKey("MockInterviewId")
      .OnDelete(DeleteBehavior.Cascade);
  }
}
