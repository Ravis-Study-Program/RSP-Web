using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class MentorshipEntity
{
  public string MentorshipId { get; set; } = string.Empty;
  public string MentorEnrollmentId { get; set; } = string.Empty;
  public string MenteeEnrollmentId { get; set; } = string.Empty;

  // Navigation
  public virtual EnrollmentEntity MentorEnrollment { get; set; } = null!;
  public virtual EnrollmentEntity MenteeEnrollment { get; set; } = null!;
}

public class MentorshipEntityConfiguration : IEntityTypeConfiguration<MentorshipEntity>
{
  public void Configure(EntityTypeBuilder<MentorshipEntity> builder)
  {
    builder.ToTable("Mentorship");
    builder.HasKey(x => x.MentorshipId);

    // Indexes
    builder.HasIndex(x => x.MentorEnrollmentId);
    builder.HasIndex(x => x.MenteeEnrollmentId);

    // Fields
    builder.Property(x => x.MentorshipId).HasColumnName("MentorshipId").HasColumnType("varchar(16)")
           .ValueGeneratedNever().IsRequired();
    builder.Property(x => x.MentorEnrollmentId).HasColumnName("MentorEnrollmentId").HasColumnType("varchar(16)")
           .IsRequired();
    builder.Property(x => x.MenteeEnrollmentId).HasColumnName("MenteeEnrollmentId").HasColumnType("varchar(16)")
           .IsRequired();

    // Foreign Keys
    builder.HasOne(x => x.MentorEnrollment)
           .WithMany()
           .HasForeignKey(x => x.MentorEnrollmentId)
           .OnDelete(DeleteBehavior.Cascade)
           .IsRequired();

    builder.HasOne(x => x.MenteeEnrollment)
           .WithMany()
           .HasForeignKey(x => x.MenteeEnrollmentId)
           .OnDelete(DeleteBehavior.Cascade)
           .IsRequired();
  }
}
