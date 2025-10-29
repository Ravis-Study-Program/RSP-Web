using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class SeasonEntity : ISoftDelete
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Slug { get; set; } = string.Empty;

  [Required]
  public DateTime StartDateInclusiveUtc { get; set; }

  [Required]
  public DateTime EndDateInclusiveUtc { get; set; }

  [Required]
  public string Location { get; set; } = string.Empty;

  [Required]
  public string ImageUrl { get; set; } = string.Empty;

  [Required]
  public string ResourcesUrl { get; set; } = string.Empty;

  [Required]
  public bool IsDataBackFilled { get; set; }

  public DateTime? DeletedAtUtc { get; set; }
}

public class SeasonEntityConfiguration : IEntityTypeConfiguration<SeasonEntity>
{
  public void Configure(EntityTypeBuilder<SeasonEntity> builder)
  {
    builder.ToTable("Season");
    builder.HasKey(x => x.SeasonId);

    // Indexes
    builder.HasIndex(x => x.Slug);

    // Fields
    builder
      .Property(x => x.SeasonId)
      .HasColumnName("SeasonId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder.Property(x => x.Name).HasColumnName("Name").HasColumnType("varchar(100)").IsRequired();
    builder.Property(x => x.Slug).HasColumnName("Slug").HasColumnType("varchar(16)").IsRequired();
    builder
      .Property(x => x.StartDateInclusiveUtc)
      .HasColumnName("StartDateInclusiveUTC")
      .HasColumnType("timestamptz")
      .IsRequired();
    builder
      .Property(x => x.EndDateInclusiveUtc)
      .HasColumnName("EndDateInclusiveUTC")
      .HasColumnType("timestamptz")
      .IsRequired();
    builder
      .Property(x => x.Location)
      .HasColumnName("Location")
      .HasColumnType("varchar(100)")
      .IsRequired();
    builder
      .Property(x => x.ImageUrl)
      .HasColumnName("ImageUrl")
      .HasColumnType("varchar(255)")
      .IsRequired();
    builder
      .Property(x => x.ResourcesUrl)
      .HasColumnName("ResourcesUrl")
      .HasColumnType("varchar(255)")
      .IsRequired();
    builder
      .Property(x => x.IsDataBackFilled)
      .HasColumnName("IsDataBackFilled")
      .IsRequired();
    builder
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");
  }
}
