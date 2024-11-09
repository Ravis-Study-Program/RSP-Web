using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class ProblemAttemptEntity
{
  [Required] public string ProblemAttemptId { get; set; } = string.Empty;
  [Required] public DateTime AttemptStartDateUtc { get; set; }
  [Required] public int TimeTakenInMinutes { get; set; }
  [Required] public string Notes { get; set; } = string.Empty;
  [Required] public string UserId { get; set; } = string.Empty;

  // The actual problem could be one of the following listed below.
  public string? LeetcodeProblemId { get; set; }
  public string? CustomProblemId { get; set; }

  // A null enrollment would mean the problem attempt is not tied to any season.
  public string? EnrollmentId { get; set; }

  // Navigation
  public UserEntity User { get; set; } = null!;
  public LeetcodeProblemEntity LeetcodeProblem { get; set; } = null!;
  public CustomProblemEntity CustomProblem { get; set; } = null!;
  public EnrollmentEntity Enrollment { get; set; } = null!;
}

public class ProblemAttemptEntityConfiguration : IEntityTypeConfiguration<ProblemAttemptEntity>
{
  public void Configure(EntityTypeBuilder<ProblemAttemptEntity> builder)
  {
    builder.ToTable("ProblemAttempt");
    builder.HasKey(x => x.ProblemAttemptId);

    // Fields
    builder.Property(x => x.ProblemAttemptId).HasColumnName("ProblemAttemptId").HasColumnType("varchar(16)")
           .HasMaxLength(32).ValueGeneratedNever().IsRequired();
    builder.Property(x => x.AttemptStartDateUtc).HasColumnName("AttemptStartDateUtc")
           .HasColumnType("timestamptz").IsRequired();
    builder.Property(x => x.TimeTakenInMinutes).HasColumnName("TimeTakenInMinutes").HasColumnType("int")
           .IsRequired();
    builder.Property(x => x.Notes).HasColumnName("Notes").HasColumnType("varchar(16)").HasMaxLength(10000).IsRequired();
    builder.Property(x => x.UserId).HasColumnName("UserId").HasColumnType("varchar(16)").HasMaxLength(32).IsRequired();
    builder.Property(x => x.LeetcodeProblemId).HasColumnName("LeetcodeProblemId").HasColumnType("varchar(16)")
           .HasMaxLength(32);
    builder.Property(x => x.CustomProblemId).HasColumnName("CustomProblemId").HasColumnType("varchar(16)")
           .HasMaxLength(32);
    builder.Property(x => x.EnrollmentId).HasColumnName("EnrollmentId").HasColumnType("varchar(16)")
           .HasMaxLength(32);

    // Foreign Keys
    builder.HasOne(x => x.User)
           .WithMany()
           .HasForeignKey(x => x.UserId)
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired();
    builder.HasOne(x => x.LeetcodeProblem)
           .WithMany()
           .HasForeignKey(x => x.LeetcodeProblemId)
           .OnDelete(DeleteBehavior.SetNull)
           .IsRequired(false);
    builder.HasOne(x => x.CustomProblem)
           .WithMany()
           .HasForeignKey(x => x.CustomProblemId)
           .OnDelete(DeleteBehavior.SetNull)
           .IsRequired(false);
    builder.HasOne(x => x.Enrollment)
           .WithMany()
           .HasForeignKey(x => x.EnrollmentId)
           .OnDelete(DeleteBehavior.SetNull)
           .IsRequired(false);
  }
}