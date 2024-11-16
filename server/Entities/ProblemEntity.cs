using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Entities.Interfaces;

namespace RSPWebAPI.Entities;

public class ProblemEntity : ISoftDelete
{
  [Required]
  public string ProblemId { get; set; } = string.Empty;

  [Required]
  public string Title { get; set; } = string.Empty;

  [Required]
  public string? Link { get; set; }
  public DateTime? DeletedAtUtc { get; set; }
}

public class ProblemEntityConfiguration : IEntityTypeConfiguration<ProblemEntity>
{
  public void Configure(EntityTypeBuilder<ProblemEntity> builder)
  {
    builder.ToTable("Problem");
    builder.HasKey(x => x.ProblemId);

    // Fields
    builder
      .Property(x => x.ProblemId)
      .HasColumnName("ProblemId")
      .HasColumnType("varchar(16)")
      .ValueGeneratedNever()
      .IsRequired();
    builder
      .Property(x => x.Title)
      .HasColumnName("Title")
      .HasColumnType("varchar(100)")
      .IsRequired();
    builder.Property(x => x.Link).HasColumnName("Link").HasColumnType("varchar(510)");
    builder
      .Property(x => x.DeletedAtUtc)
      .HasColumnName("DeletedAtUtc")
      .HasColumnType("timestamptz");
  }
}
