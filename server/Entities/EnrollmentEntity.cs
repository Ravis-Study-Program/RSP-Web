using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class EnrollmentEntity : IBaseEntity, ISoftDelete
{
  [Required]
  public string EnrollmentId { get; set; } = string.Empty;

  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public SeasonRole Role { get; set; }

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }

  // Navigation
  public SeasonEntity Season { get; set; } = null!;
  public UserEntity User { get; set; } = null!;

  [Required]
  public DateTime CreatedAtUtc { get; set; }

  [Required]
  public DateTime UpdatedAtUtc { get; set; }

  public DateTime? DeletedAtUtc { get; set; }
}

public class EnrollmentEntityConfiguration : IEntityTypeConfiguration<EnrollmentEntity>
{
  public void Configure(EntityTypeBuilder<EnrollmentEntity> builder)
  {
    builder.ToTable("Enrollment");
    builder.HasKey(x => x.EnrollmentId);

    // Indexes
    builder.HasIndex(x => x.SeasonId);
    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => new { x.SeasonId, x.UserId });

    // Fields
    builder
      .Property(x => x.EnrollmentId)
      .HasColumnName("EnrollmentId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder
      .Property(x => x.SeasonId)
      .HasColumnName("SeasonId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.UserId)
      .HasColumnName("UserId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder.Property(x => x.Role).HasColumnName("Role").IsRequired();
    builder
      .Property(x => x.CreatedAtUtc)
      .HasColumnName("CreatedAtUtc")
      .HasColumnType("timestamptz")
      .IsRequired();
    builder
      .Property(x => x.UpdatedAtUtc)
      .HasColumnName("UpdatedAtUtc")
      .HasColumnType("timestamptz")
      .IsRequired();
    builder
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");
    builder
      .Property(x => x.StudentRolePromotion)
      .HasColumnName("StudentRolePromotion")
      .HasColumnType("int")
      .IsRequired();

    // Foreign Keys
    builder
      .HasOne(x => x.Season)
      .WithMany()
      .HasForeignKey(x => x.SeasonId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
    builder
      .HasOne(x => x.User)
      .WithMany()
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Restrict)
      .IsRequired();
  }
}
