using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class MockInterviewEntity
{
  [Required] public string MockInterviewId { get; set; } = string.Empty;
  [Required] public bool IsPass { get; set; }
  [Required] public DateTime StartDate { get; set; }
  [Required] public int TimeTakenInMinutes { get; set; }

  [Required] public string InterviewerUserId { get; set; } = string.Empty;
  [Required] public string IntervieweeUserId { get; set; } = string.Empty;

  // A null enrollment would mean the problem attempt is not tied to any season.
  public string? EnrollmentId { get; set; } = string.Empty;

  // Navigation
  public UserEntity Interviewer { get; set; } = null!;
  public UserEntity Interviewee { get; set; } = null!;
  public EnrollmentEntity Enrollment { get; set; } = null!;
  public ICollection<MockInterviewRoundEntity> MockInterviewRounds { get; set; } = null!;
}

public class MockInterviewEntityConfiguration : IEntityTypeConfiguration<MockInterviewEntity>
{
  public void Configure(EntityTypeBuilder<MockInterviewEntity> builder)
  {
    builder.ToTable("MockInterview");
    builder.HasKey(x => x.MockInterviewId);

    // Fields  
    builder.Property(x => x.MockInterviewId).HasColumnName("MockInterviewId").HasColumnType("varchar(16)")
           .HasMaxLength(32)
           .ValueGeneratedNever().IsRequired();
    builder.Property(x => x.IsPass).HasColumnName("IsPass").IsRequired();
    builder.Property(x => x.StartDate).HasColumnName("StartDate").HasColumnType("timestamptz").IsRequired();
    builder.Property(x => x.TimeTakenInMinutes).HasColumnName("TimeTakenInMinutes").HasColumnType("int").IsRequired();
    builder.Property(x => x.EnrollmentId).HasColumnName("EnrollmentId").HasColumnType("varchar(16)").HasMaxLength(32);
    builder.Property(x => x.InterviewerUserId).HasColumnName("InterviewerUserId").HasColumnType("varchar(16)")
           .HasMaxLength(32).IsRequired();
    builder.Property(x => x.IntervieweeUserId).HasColumnName("IntervieweeUserId").HasColumnType("varchar(16)")
           .HasMaxLength(32).IsRequired();

    // Foreign Keys
    builder.HasOne(x => x.Interviewer).WithMany().HasForeignKey(x => x.InterviewerUserId)
           .OnDelete(DeleteBehavior.Restrict).IsRequired();
    builder.HasOne(x => x.Interviewee).WithMany().HasForeignKey(x => x.IntervieweeUserId)
           .OnDelete(DeleteBehavior.Restrict).IsRequired();
    builder.HasOne(x => x.Enrollment).WithMany().HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.SetNull)
           .IsRequired(false);

    // Collection Relationship
    builder.HasMany(x => x.MockInterviewRounds).WithOne().HasForeignKey("MockInterviewId")
           .OnDelete(DeleteBehavior.Cascade);
  }
}
