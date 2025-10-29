using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class KickStudentEventEntity : ISoftDelete
{
  [Required]
  public string KickStudentEventId { get; set; } = string.Empty;

  [Required]
  public DateTime KickedAtUtc { get; set; }

  [Required]
  public string MentorId { get; set; } = string.Empty;

  [Required]
  public string StudentId { get; set; } = string.Empty;

  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string KickReason { get; set; } = string.Empty;

  // Navigation
  public UserEntity Mentor { get; set; } = null!;
  public UserEntity Student { get; set; } = null!;
  public SeasonEntity Season { get; set; } = null!;
  public DateTime? DeletedAtUtc { get; set; }
}

public class KickStudentEventEntityConfiguration : IEntityTypeConfiguration<KickStudentEventEntity>
{
  public void Configure(EntityTypeBuilder<KickStudentEventEntity> builder)
  {
    builder.ToTable("KickStudentEvent");
    builder.HasKey(x => x.KickStudentEventId);

    // Indexes
    builder.HasIndex(x => x.MentorId);
    builder.HasIndex(x => x.StudentId);
    builder.HasIndex(x => x.SeasonId);
    builder.HasIndex(x => x.KickedAtUtc);
    builder.HasIndex(x => new { x.StudentId, x.KickedAtUtc });
    builder.HasIndex(x => new { x.SeasonId, x.KickedAtUtc });

    // Fields
    builder
      .Property(x => x.KickStudentEventId)
      .HasColumnName("KickStudentEventId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder
      .Property(x => x.KickedAtUtc)
      .HasColumnName("KickedAtUtc")
      .HasColumnType("timestamptz")
      .IsRequired();
    builder
      .Property(x => x.MentorId)
      .HasColumnName("MentorId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.StudentId)
      .HasColumnName("StudentId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.SeasonId)
      .HasColumnName("SeasonId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.KickReason)
      .HasColumnName("KickReason")
      .HasColumnType("text")
      .IsRequired();
    builder
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");

    // Foreign Keys
    builder
      .HasOne(x => x.Mentor)
      .WithMany()
      .HasForeignKey(x => x.MentorId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
    builder
      .HasOne(x => x.Student)
      .WithMany()
      .HasForeignKey(x => x.StudentId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
    builder
      .HasOne(x => x.Season)
      .WithMany()
      .HasForeignKey(x => x.SeasonId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
  }
}