using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class UserEntity : ISoftDelete
{
  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public string DiscordId { get; set; } = string.Empty;

  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public bool IsAdmin { get; set; }

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Slug { get; set; } = string.Empty;

  [Required]
  public string ProfileImage { get; set; } = string.Empty;
  public DateTime? DeletedAtUtc { get; set; }
}

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
  public void Configure(EntityTypeBuilder<UserEntity> builder)
  {
    builder.ToTable("User");
    builder.HasKey(x => x.UserId);

    // Indexes
    builder.HasIndex(x => x.Email);
    builder.HasIndex(x => x.Slug);

    // Fields
    builder
      .Property(x => x.UserId)
      .HasColumnName("UserId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder
      .Property(x => x.DiscordId)
      .HasColumnName("DiscordId")
      .HasColumnType("varchar(16)")
      .IsRequired();
    builder
      .Property(x => x.Email)
      .HasColumnName("Email")
      .HasColumnType("varchar(100)")
      .IsRequired();
    builder.Property(x => x.IsAdmin).HasColumnName("IsAdmin").IsRequired();
    builder.Property(x => x.Name).HasColumnName("Name").HasColumnType("varchar(100)").IsRequired();
    builder.Property(x => x.Slug).HasColumnName("Slug").HasColumnType("varchar(100)").IsRequired();
    builder
      .Property(x => x.ProfileImage)
      .HasColumnName("ProfileImage")
      .HasColumnType("varchar(255)")
      .IsRequired();
    builder
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");
  }
}
